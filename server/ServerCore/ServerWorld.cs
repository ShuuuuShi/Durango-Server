using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Durango.Network;
using Durango.Offline;
using Durango.Utils;
using Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Shared.Item;
using Shared.Region;
using Shared.Economy;
using Shared.Faction;
using Shared.Skill;
using Shared.Social;
using Shared.Building;
using Shared.Etc;

namespace DurangoServer.Core;

// ============================================================================
// DurangoServer — ไฟล์หลักของ server
// ประกอบด้วย: ServerWorld (โลก), ServerPlayer (ผู้เล่น + handler เกมเพลย์),
// GameServer (TCP 8191), Gateway (HTTP 8190 + UDP knock), RadiotowerServer (แชท 8192)
// โปรโตคอล: MsgPack + Snappy, header 24 ไบต์ (time/seq/replyOf/typeCode/size)
// ============================================================================

// ServerWorld — ดูรายละเอียดที่ docs/server/ServerWorld.md

public class ServerWorld
{
    public TerrainStore Terrain { get; }
    public string ServerName { get; }
    public Point2 EntryPoint => Terrain.EntryPoint;

    private readonly object _lock = new object();
    private readonly List<ServerPlayer> _players = new List<ServerPlayer>();

    // GP-03: state ของจุดเก็บของในธรรมชาติ (key = entity id เช่น "natural_120_88")
    // เดิมอยู่ใน ServerPlayer = แยกกันคนละชุดต่อผู้เล่น ทำให้ 2 คนเก็บต้นเดียวกัน
    // ได้ของครบทั้งคู่ (ก๊อปของ) และคนที่เก็บหมดก่อนสั่งลบต้นไม้ทิ้งขณะที่อีกคนยังเก็บต่อได้
    private readonly Dictionary<string, List<Generator>> _generators = new Dictionary<string, List<Generator>>();
    private readonly object _genLock = new object();

    // GP-09: entity id ของธรรมชาติ → tile ที่มันอยู่จริง (ผูกตอน Touch หลังเช็คกับ garden แล้ว)
    // เดิม Collect เชื่อ Tile ที่ client แนบมา ทำให้สั่งลบต้นไม้ที่พิกัดไหนก็ได้
    private readonly Dictionary<string, Point2> _naturalTiles = new Dictionary<string, Point2>();

    // GP-04: สิ่งปลูกสร้างทั้งหมดในโลก (key = entity id)
    // เดิมไม่มีที่เก็บเลย — broadcast แล้วทิ้ง ทำให้คนที่เข้ามาทีหลังไม่เห็นบ้านที่สร้างไปก่อนหน้า
    // และตรวจสิทธิ์ตอนทุบไม่ได้เพราะไม่รู้ว่าใครสร้าง
    private readonly Dictionary<string, AppearArtifact> _artifacts = new Dictionary<string, AppearArtifact>();
    private readonly object _artifactLock = new object();

    // เฟส C — ของในกล่องเก็บของ (key = entity id ของสิ่งปลูกสร้าง)
    private readonly Dictionary<string, List<Item>> _boxes = new Dictionary<string, List<Item>>();
    private readonly object _boxLock = new object();

    // GP-07: blueprint id ของแต่ละ artifact — ไม่มีใน AppearArtifact แต่จำเป็นตอนสร้างกลับจากเซฟ
    // (ใช้หา default look / component ว่าเป็น Burnable ไหม)
    private readonly Dictionary<string, string> _artifactBlueprints = new Dictionary<string, string>();

    // GP-07: มีอะไรเปลี่ยนตั้งแต่เซฟครั้งล่าสุดไหม — autosave จะข้ามถ้าไม่มีอะไรเปลี่ยน
    private volatile bool _dirty;

    public bool IsDirty => _dirty;

    public void MarkDirty()
    {
        _dirty = true;
    }

    /// <summary>เฟส C — ตัวจัดการสัตว์ในโลก</summary>
    public AnimalSpawner Animals { get; }

    public ServerWorld(TerrainStore terrain, string serverName)
    {
        Terrain = terrain;
        ServerName = serverName;
        Animals = new AnimalSpawner(this);
    }

    // จุดเกิด = entry point ของ terrain (1 tile = 200 หน่วย client)
    public WorldPosition GetEntryPosition()
    {
        return new WorldPosition(EntryPoint.x * 200f, EntryPoint.y * 200f);
    }

    // ผู้เล่นใหม่เข้ามา: ส่งสิ่งปลูกสร้าง + AppearPlayer ของคนเก่าให้คนใหม่ แล้ว broadcast ตัวคนใหม่ให้คนอื่น
    public void AddPlayer(ServerPlayer player)
    {
        // GP-04: ส่งสิ่งปลูกสร้างที่มีอยู่แล้วทั้งหมดให้คนใหม่
        // ทำนอก _lock เพราะใช้ล็อกคนละตัว และไม่ต้องถือ _lock ตอนทำ I/O
        AppearArtifact[] artifacts = SnapshotArtifacts();
        for (int i = 0; i < artifacts.Length; i++)
        {
            player.Send(artifacts[i]);
        }

        // เฟส C: ส่งสัตว์ที่มีอยู่ในโลกให้คนใหม่ด้วย
        Animals.SendAllTo(player);

        lock (_lock)
        {
            foreach (ServerPlayer other in _players)
            {
                player.Send(other.MakeAppearPlayer());
            }
            _players.Add(player);
        }
        BroadcastExcept(player, player.MakeAppearPlayer());
        Console.WriteLine($"[world] player joined: {player.EntityId} ({player.Name}), total={Count}, artifacts={artifacts.Length}, สัตว์={Animals.Count}");
    }

    public void RemovePlayer(ServerPlayer player)
    {
        lock (_lock)
        {
            _players.Remove(player);
        }
        Broadcast(new DisappearEntity { EntityId = player.EntityId });
        Console.WriteLine($"[world] player left: {player.EntityId} ({player.Name}), total={Count}");
    }

    /// <summary>
    /// ความสูงพื้นล่าสุดที่ client รายงานมา — server ไม่มี heightmap ของแมพเอง
    /// ใช้เป็นค่าประมาณตอนวางสัตว์ ไม่งั้นสัตว์จะจมใต้พื้น (เห็นแต่เงา)
    /// </summary>
    public float GroundHeightHint { get; private set; }

    public void NoteGroundHeight(float height)
    {
        if (height != 0f)
        {
            GroundHeightHint = height;
        }
    }

    /// <summary>หาผู้เล่นที่ออนไลน์อยู่จาก entity id (null ถ้าไม่มี) — เฟส C รอบ 2 ใช้ตอนต่อสู้</summary>
    /// <summary>สำเนารายชื่อผู้เล่นที่ออนไลน์ (คืนสำเนาเสมอ ผู้เรียกจะได้ไม่ต้องถือ lock ต่อ)</summary>
    public ServerPlayer[] SnapshotPlayers()
    {
        lock (_lock)
        {
            return _players.ToArray();
        }
    }

    public ServerPlayer FindPlayer(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return null;
        }
        lock (_lock)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                if (_players[i].EntityId == entityId)
                {
                    return _players[i];
                }
            }
        }
        return null;
    }

    /// <summary>หาผู้เล่นจาก entity id หรือ "ชื่อที่ขึ้นต้นด้วย" (ใช้กับคำสั่งรีโมทคุมตัวละคร)</summary>
    public ServerPlayer FindPlayerByNameOrId(string key)
    {
        if (string.IsNullOrEmpty(key))
        {
            return null;
        }
        ServerPlayer exact = FindPlayer(key);
        if (exact != null)
        {
            return exact;
        }
        lock (_lock)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                string name = _players[i].Name ?? string.Empty;
                if (name.StartsWith(key, StringComparison.OrdinalIgnoreCase) || name.Trim() == key)
                {
                    return _players[i];
                }
            }
        }
        return null;
    }

    /// <summary>ผู้เล่นที่ยังไม่ตายที่ใกล้จุดนี้ที่สุดภายในรัศมี (null ถ้าไม่มีใคร) — ใช้กับ AI สัตว์ดุ</summary>
    public ServerPlayer FindNearestPlayer(WorldPosition pos, float maxDistance)
    {
        ServerPlayer best = null;
        float bestDist = maxDistance;
        lock (_lock)
        {
            for (int i = 0; i < _players.Count; i++)
            {
                ServerPlayer p = _players[i];
                if (p.Dead)
                {
                    continue;
                }
                WorldPosition pp = p.CurrentPosition;
                float dx = pp.x - pos.x;
                float dy = pp.y - pos.y;
                float d = MathF.Sqrt(dx * dx + dy * dy);
                if (d <= bestDist)
                {
                    bestDist = d;
                    best = p;
                }
            }
        }
        return best;
    }

    /// <summary>มีสิ่งปลูกสร้างวางทับ tile นี้อยู่แล้วไหม (H-7)</summary>
    public bool HasArtifactAt(Point2 tile)
    {
        lock (_artifactLock)
        {
            foreach (AppearArtifact a in _artifacts.Values)
            {
                int sx = a.Size.x <= 0 ? 1 : a.Size.x;
                int sy = a.Size.y <= 0 ? 1 : a.Size.y;
                if (tile.x >= a.Tile.x && tile.x < a.Tile.x + sx
                    && tile.y >= a.Tile.y && tile.y < a.Tile.y + sy)
                {
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>ผู้เล่นคนนี้สร้างไว้กี่ชิ้นแล้ว (H-7 — ใช้บังคับเพดานต่อคน)</summary>
    public int CountArtifactsOf(string entityId)
    {
        int n = 0;
        lock (_artifactLock)
        {
            foreach (AppearArtifact a in _artifacts.Values)
            {
                if (a.FounderEntityId == entityId)
                {
                    n++;
                }
            }
        }
        return n;
    }

    /// <summary>
    /// เดินระบบเอาชีวิตรอดของทุกคน (ฟื้นสตามินา · พักที่กองไฟ · เลือดไหลตอนล้าเต็ม)
    /// เรียกวินาทีละครั้งพอ — ค่าทั้งหมดเป็น gauge ที่ client interpolate เองอยู่แล้ว
    /// ไม่ต้องคิดทุก tick (120 ครั้ง/วิ) ซึ่งเปลืองเปล่า ๆ
    /// </summary>
    public void TickSurvival(double now)
    {
        if (now < _nextSurvivalTickAt)
        {
            return;
        }
        _nextSurvivalTickAt = now + 1.0;
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        for (int i = 0; i < snapshot.Length; i++)
        {
            snapshot[i].TickSurvival(now);
        }
    }

    private double _nextSurvivalTickAt;

    public void Broadcast<T>(T msg) where T : struct
    {
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        foreach (ServerPlayer p in snapshot)
        {
            p.Send(msg);
        }
    }

    // GP-13: เดิมมีพารามิเตอร์ bool excludeSelf ที่ไม่เคยถูกอ่านในบอดี้เลย (ตัดสินจาก p == except อย่างเดียว)
    // พฤติกรรมถูกอยู่แล้ว แต่ทำให้คนอ่านเข้าใจผิดว่ามีสวิตช์ จึงเอาออก
    public void BroadcastExcept<T>(ServerPlayer except, T msg) where T : struct
    {
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        foreach (ServerPlayer p in snapshot)
        {
            if (p == except)
            {
                continue;
            }
            p.Send(msg);
        }
    }

    public int Count
    {
        get
        {
            lock (_lock)
            {
                return _players.Count;
            }
        }
    }

    /// <summary>GP-04: จำสิ่งปลูกสร้างไว้ในโลก (เรียกทุกครั้งที่มีการสร้าง/วางของ)</summary>
    public void AddArtifact(AppearArtifact artifact, string blueprintId = null)
    {
        if (string.IsNullOrEmpty(artifact.EntityId))
        {
            return;
        }
        lock (_artifactLock)
        {
            _artifacts[artifact.EntityId] = artifact;
            if (!string.IsNullOrEmpty(blueprintId))
            {
                // GP-07: blueprint id ไม่ได้อยู่ใน AppearArtifact แต่ต้องใช้ตอนสร้างกลับจากเซฟ
                _artifactBlueprints[artifact.EntityId] = blueprintId;
            }
        }
        MarkDirty();
    }

    /// <summary>
    /// มีที่พักผ่อน (กองไฟ/เต็นท์) อยู่ใกล้ตรงนี้ไหม — ใช้กับ RestOn
    ///
    /// ถ้า client ระบุ id มาก็ตรวจอันนั้น แต่**ยังต้องเช็คระยะจริงเสมอ** (ห้ามเชื่อ client)
    /// ไม่ได้ระบุมา ก็ค้นให้เองว่ามีอะไรพักได้อยู่ในระยะ
    /// </summary>
    public bool IsRestSpotNear(string artifactId, WorldPosition from, float range, out string spotName)
    {
        spotName = null;
        lock (_artifactLock)
        {
            foreach (KeyValuePair<string, AppearArtifact> pair in _artifacts)
            {
                if (!string.IsNullOrEmpty(artifactId) && pair.Key != artifactId)
                {
                    continue;
                }
                _artifactBlueprints.TryGetValue(pair.Key, out string blueprint);
                if (!IsRestBlueprint(blueprint))
                {
                    continue;
                }
                float ax = pair.Value.Tile.x * 200f + 100f;
                float ay = pair.Value.Tile.y * 200f + 100f;
                float dx = ax - from.x, dy = ay - from.y;
                if (dx * dx + dy * dy <= range * range)
                {
                    spotName = blueprint != null && blueprint.Contains("tent") ? "เต็นท์" : "กองไฟ";
                    return true;
                }
            }
        }
        return false;
    }

    /// <summary>สิ่งปลูกสร้างชนิดนี้ใช้พักผ่อนได้ไหม (ตามรายการ beta: กองไฟ · เต็นท์ · ที่หลับนอน)</summary>
    private static bool IsRestBlueprint(string blueprintId)
    {
        if (string.IsNullOrEmpty(blueprintId))
        {
            return false;
        }
        string b = blueprintId.ToLowerInvariant();
        return b.Contains("bonfire") || b.Contains("campfire") || b.Contains("tent") || b.Contains("hammock");
    }

    public bool TryGetArtifact(string entityId, out AppearArtifact artifact)
    {
        lock (_artifactLock)
        {
            return _artifacts.TryGetValue(entityId ?? string.Empty, out artifact);
        }
    }

    public bool RemoveArtifact(string entityId)
    {
        bool removed;
        lock (_artifactLock)
        {
            removed = _artifacts.Remove(entityId ?? string.Empty);
            _artifactBlueprints.Remove(entityId ?? string.Empty);
        }
        // เฟส C: ทุบกล่องแล้วของข้างในหายไปด้วย (ยังไม่มีระบบดรอปของลงพื้น)
        lock (_boxLock)
        {
            _boxes.Remove(entityId ?? string.Empty);
        }
        if (removed)
        {
            MarkDirty();
        }
        return removed;
    }

    /// <summary>เปลี่ยนสถานะการสร้าง (Occupied → Built) ให้คนที่เข้ามาทีหลังเห็นสถานะถูกต้อง</summary>
    public void SetArtifactBuildingState(string entityId, BuildingState state)
    {
        lock (_artifactLock)
        {
            if (_artifacts.TryGetValue(entityId ?? string.Empty, out AppearArtifact a))
            {
                ArtifactState states = a.States;
                states.BuildingState = state;
                a.States = states;
                _artifacts[entityId] = a;
            }
        }
        MarkDirty();
    }

    // ---------------------------------------------------------------- GP-07: เซฟ/โหลดโลก

    /// <summary>เขียนสิ่งปลูกสร้าง + ต้นไม้ที่ถูกเก็บไปแล้ว ลงดิสก์</summary>
    public bool Save()
    {
        WorldSave save = new WorldSave
        {
            TerrainId = Terrain.TerrainId,
            RemovedNaturals = Terrain.GetRemovedNaturals()
        };
        lock (_artifactLock)
        {
            foreach (KeyValuePair<string, AppearArtifact> pair in _artifacts)
            {
                _artifactBlueprints.TryGetValue(pair.Key, out string blueprintId);
                save.Artifacts.Add(ArtifactSave.From(pair.Value, blueprintId));
            }
        }
        // เฟส C — ของในกล่อง
        foreach (KeyValuePair<string, List<Item>> pair in SnapshotBoxes())
        {
            var list = new List<ItemSave>(pair.Value.Count);
            for (int i = 0; i < pair.Value.Count; i++)
            {
                list.Add(ItemSave.From(pair.Value[i]));
            }
            save.Boxes[pair.Key] = list;
        }

        bool ok = SaveStore.Save(SaveStore.WorldPath, save);
        if (ok)
        {
            _dirty = false;
        }
        return ok;
    }

    /// <summary>โหลดโลกกลับมาตอนเปิดเซิร์ฟ เรียกก่อนรับ client</summary>
    public void Load()
    {
        WorldSave save = SaveStore.Load<WorldSave>(SaveStore.WorldPath);
        if (save == null)
        {
            Console.WriteLine("[save] ยังไม่มีไฟล์เซฟโลก — เริ่มจากแมพเปล่า");
            return;
        }
        if (!string.IsNullOrEmpty(save.TerrainId) && save.TerrainId != Terrain.TerrainId)
        {
            Console.WriteLine($"[save] ⚠️ เซฟเป็นแมพ '{save.TerrainId}' แต่ตอนนี้โหลดแมพ '{Terrain.TerrainId}' — ตำแหน่งของอาจเพี้ยน");
        }

        int naturals = Terrain.ApplyRemovedNaturals(save.RemovedNaturals);

        int loaded = 0;
        lock (_artifactLock)
        {
            for (int i = 0; i < save.Artifacts.Count; i++)
            {
                ArtifactSave a = save.Artifacts[i];
                if (a == null || string.IsNullOrEmpty(a.EntityId))
                {
                    continue;
                }
                try
                {
                    _artifacts[a.EntityId] = a.ToArtifact();
                    if (!string.IsNullOrEmpty(a.BlueprintId))
                    {
                        _artifactBlueprints[a.EntityId] = a.BlueprintId;
                    }
                    loaded++;
                }
                catch (Exception e)
                {
                    // artifact ตัวเดียวพังไม่ควรทำให้โลกทั้งใบโหลดไม่ได้
                    Console.WriteLine($"[save] ข้าม artifact {a.EntityId}: {e.Message}");
                }
            }
        }
        // เฟส C — ของในกล่อง
        int boxes = 0;
        if (save.Boxes != null)
        {
            foreach (KeyValuePair<string, List<ItemSave>> pair in save.Boxes)
            {
                if (pair.Value == null || pair.Value.Count == 0)
                {
                    continue;
                }
                var items = new List<Item>(pair.Value.Count);
                for (int i = 0; i < pair.Value.Count; i++)
                {
                    if (pair.Value[i] != null && !string.IsNullOrEmpty(pair.Value[i].Id))
                    {
                        items.Add(pair.Value[i].ToItem());
                    }
                }
                RestoreBox(pair.Key, items);
                boxes++;
            }
        }

        _dirty = false;
        Console.WriteLine($"[save] โหลดโลกแล้ว: สิ่งปลูกสร้าง {loaded} ชิ้น, ธรรมชาติที่ถูกเก็บไปแล้ว {naturals} จุด, กล่องที่มีของ {boxes} ใบ");
    }

    /// <summary>สิ่งปลูกสร้างทั้งหมด ณ ตอนนี้ — ใช้ส่งให้ผู้เล่นที่เพิ่งเข้ามา</summary>
    public AppearArtifact[] SnapshotArtifacts()
    {
        lock (_artifactLock)
        {
            AppearArtifact[] all = new AppearArtifact[_artifacts.Count];
            _artifacts.Values.CopyTo(all, 0);
            return all;
        }
    }

    public int ArtifactCount
    {
        get
        {
            lock (_artifactLock)
            {
                return _artifacts.Count;
            }
        }
    }

    /// <summary>blueprint ของสิ่งปลูกสร้างชิ้นนี้ (ใช้ดูว่าเป็นโต๊ะคราฟต์ชนิดไหน)</summary>
    public bool TryGetArtifactBlueprint(string entityId, out string blueprintId)
    {
        lock (_artifactLock)
        {
            if (!_artifacts.ContainsKey(entityId ?? string.Empty))
            {
                blueprintId = null;
                return false;
            }
            return _artifactBlueprints.TryGetValue(entityId, out blueprintId);
        }
    }

    // ---------------------------------------------------------------- เฟส C: กล่องเก็บของ

    /// <summary>สิ่งปลูกสร้างนี้เป็นกล่องเก็บของไหม (blueprint มี component "Inventory")</summary>
    public bool IsStorage(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return false;
        }
        string blueprintId;
        lock (_artifactLock)
        {
            if (!_artifacts.ContainsKey(entityId))
            {
                return false;
            }
            _artifactBlueprints.TryGetValue(entityId, out blueprintId);
        }
        if (string.IsNullOrEmpty(blueprintId))
        {
            return false;
        }
        if (!RecipeData.BlueprintComponents.TryGetValue(blueprintId, out string[] comps))
        {
            return false;
        }
        return Array.IndexOf(comps, "Inventory") != -1;
    }

    public Item[] GetBoxItems(string boxId)
    {
        lock (_boxLock)
        {
            return _boxes.TryGetValue(boxId ?? string.Empty, out List<Item> items)
                ? items.ToArray()
                : Array.Empty<Item>();
        }
    }

    /// <summary>ใส่ของลงกล่อง คืน false ถ้าเต็ม (ไม่ใส่อะไรเลย ผู้เรียกต้องคืนของให้เจ้าของ)</summary>
    public bool TryPutInBox(string boxId, List<Item> items, int maxSize)
    {
        lock (_boxLock)
        {
            if (!_boxes.TryGetValue(boxId, out List<Item> box))
            {
                box = new List<Item>();
                _boxes[boxId] = box;
            }
            if (box.Count + items.Count > maxSize)
            {
                return false;
            }
            box.AddRange(items);
        }
        MarkDirty();
        return true;
    }

    /// <summary>หยิบของออกจากกล่องตาม id ที่ขอ ไม่เกิน limit ชิ้น</summary>
    public List<Item> TakeFromBox(string boxId, string[] itemIds, int limit)
    {
        var taken = new List<Item>();
        lock (_boxLock)
        {
            if (!_boxes.TryGetValue(boxId ?? string.Empty, out List<Item> box))
            {
                return taken;
            }
            for (int i = 0; i < itemIds.Length && taken.Count < limit; i++)
            {
                int idx = box.FindIndex(x => x.Id == itemIds[i]);
                if (idx >= 0)
                {
                    taken.Add(box[idx]);
                    box.RemoveAt(idx);
                }
            }
        }
        if (taken.Count > 0)
        {
            MarkDirty();
        }
        return taken;
    }

    /// <summary>ของในกล่องทั้งหมด (ใช้ตอนเซฟ)</summary>
    public Dictionary<string, List<Item>> SnapshotBoxes()
    {
        lock (_boxLock)
        {
            var copy = new Dictionary<string, List<Item>>();
            foreach (KeyValuePair<string, List<Item>> pair in _boxes)
            {
                if (pair.Value.Count > 0)
                {
                    copy[pair.Key] = new List<Item>(pair.Value);
                }
            }
            return copy;
        }
    }

    public void RestoreBox(string boxId, List<Item> items)
    {
        lock (_boxLock)
        {
            _boxes[boxId] = items;
        }
    }

    /// <summary>
    /// GP-03: ขอ generator ของจุดนี้ ยังไม่เคยมีก็สร้างใหม่จาก factory
    /// คืน "สำเนา" เสมอ เพื่อไม่ให้ผู้เรียกไปแก้ของกลางโดยไม่ผ่าน lock
    /// </summary>
    public Generator[] GetOrCreateGenerators(string naturalId, ushort entityType, Func<ushort, List<Generator>> factory)
    {
        lock (_genLock)
        {
            if (!_generators.TryGetValue(naturalId, out List<Generator> gens))
            {
                gens = factory(entityType);
                _generators[naturalId] = gens;
            }
            return gens.ToArray();
        }
    }

    /// <summary>
    /// GP-09: ผูก entity id ของธรรมชาติเข้ากับ tile ที่ตรวจแล้วว่ามีของอยู่จริง
    /// เรียกจาก HandleTouch เท่านั้น — Collect/DisappearEntityOnTile จะอ่านค่าจากที่นี่แทน Tile ของ client
    /// </summary>
    public void RegisterNaturalTile(string naturalId, Point2 tile)
    {
        if (string.IsNullOrEmpty(naturalId))
        {
            return;
        }
        lock (_genLock)
        {
            _naturalTiles[naturalId] = tile;
        }
    }

    /// <summary>GP-09: tile ของธรรมชาติชิ้นนี้ตามที่ server จำไว้</summary>
    public bool TryGetNaturalTile(string naturalId, out Point2 tile)
    {
        tile = default;
        if (string.IsNullOrEmpty(naturalId))
        {
            return false;
        }
        lock (_genLock)
        {
            return _naturalTiles.TryGetValue(naturalId, out tile);
        }
    }

    /// <summary>GP-09: ลืม entity id นี้ทิ้ง (จุดนี้ถูกเก็บหมด/ถูกลบไปแล้ว)</summary>
    public void ForgetNaturalTile(string naturalId)
    {
        if (string.IsNullOrEmpty(naturalId))
        {
            return;
        }
        lock (_genLock)
        {
            _naturalTiles.Remove(naturalId);
        }
    }

    /// <summary>ดู generator ปัจจุบันของจุดนี้ คืน null ถ้ายังไม่เคยมีใครแตะ</summary>
    public Generator[] PeekGenerators(string naturalId)
    {
        lock (_genLock)
        {
            return _generators.TryGetValue(naturalId, out List<Generator> gens) ? gens.ToArray() : null;
        }
    }

    /// <summary>
    /// GP-03: จอง 1 หน่วยจาก generator แบบอะตอมมิก — หักจำนวนทันทีที่ขอ ไม่ใช่ตอนเก็บเสร็จ
    /// ทำให้สองคนที่กดพร้อมกันบนหน่วยสุดท้าย มีแค่คนเดียวที่ผ่าน
    /// </summary>
    /// <param name="ranOut">true ถ้าหน่วยนี้เป็นหน่วยสุดท้าย (จุดนี้หมดแล้ว)</param>
    public bool TryReserveGenerator(string naturalId, string generatorId, out Generator generator, out bool ranOut)
    {
        generator = default;
        ranOut = false;
        lock (_genLock)
        {
            if (!_generators.TryGetValue(naturalId, out List<Generator> gens))
            {
                return false;
            }
            int idx = gens.FindIndex(g => g.Id == generatorId);
            if (idx < 0)
            {
                return false;
            }
            Generator g = gens[idx];
            if (g.Amount <= 0)
            {
                return false;
            }
            generator = g;
            ranOut = g.Amount <= 1;
            if (ranOut)
            {
                _generators.Remove(naturalId);
            }
            else
            {
                g.Amount -= 1;
                gens[idx] = g;
            }
            return true;
        }
    }

    /// <summary>
    /// จองของจากซากสัตว์ 1 หน่วย — ต่างจาก <see cref="TryReserveGenerator"/> ตรงที่
    /// "ชิ้นส่วนหนึ่งหมด" ไม่ได้แปลว่าซากหมด (ซากมีทั้งเนื้อ/หนัง/กระดูก แยกกัน)
    /// ของธรรมชาติหมดทีเดียวทั้งจุดเพราะต้นไม้ต้นนั้นหายไปเลย แต่ซากต้องแล่ต่อได้จนครบทุกชิ้น
    /// </summary>
    /// <param name="emptied">true ถ้าหน่วยนี้เป็นหน่วยสุดท้ายของทั้งซาก (แล่หมดตัวแล้ว)</param>
    public bool TryReserveCorpsePart(string corpseId, string generatorId, out Generator generator, out bool emptied)
    {
        generator = default;
        emptied = false;
        lock (_genLock)
        {
            if (!_generators.TryGetValue(corpseId ?? string.Empty, out List<Generator> gens))
            {
                return false;
            }
            int idx = gens.FindIndex(g => g.Id == generatorId);
            if (idx < 0)
            {
                return false;
            }
            Generator g = gens[idx];
            if (g.Amount <= 0)
            {
                return false;
            }
            generator = g;
            g.Amount -= 1;
            if (g.Amount <= 0)
            {
                gens.RemoveAt(idx);
            }
            else
            {
                gens[idx] = g;
            }
            if (gens.Count == 0)
            {
                _generators.Remove(corpseId);
                emptied = true;
            }
            return true;
        }
    }

    /// <summary>ทิ้ง generator ของ entity นี้ (ซากหมดเวลาแล้วหายไปจากโลก)</summary>
    public void ForgetGenerators(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return;
        }
        lock (_genLock)
        {
            _generators.Remove(entityId);
        }
    }

    /// <summary>ตั้ง generator ของ entity นี้ทับของเดิม (ใช้ตอนสัตว์ตาย = เปิดให้แล่ได้)</summary>
    public void SetGenerators(string entityId, List<Generator> gens)
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return;
        }
        lock (_genLock)
        {
            _generators[entityId] = gens;
        }
    }

    /// <summary>
    /// GP-07: เซฟทุกอย่างที่ค้างอยู่ (โลก + ผู้เล่นที่ยังออนไลน์)
    /// เรียกจาก autosave และตอนปิดเซิร์ฟ คืนจำนวนไฟล์ที่เขียนไป
    /// </summary>
    public int SaveAll(bool force = false)
    {
        int written = 0;
        if (force || _dirty)
        {
            if (Save())
            {
                written++;
            }
        }
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        for (int i = 0; i < snapshot.Length; i++)
        {
            if (!force && !snapshot[i].IsDirty)
            {
                continue;
            }
            try
            {
                if (snapshot[i].Save())
                {
                    written++;
                }
            }
            catch (Exception e)
            {
                Console.WriteLine($"[save] เซฟ {snapshot[i].EntityId} ไม่สำเร็จ: {e.Message}");
            }
        }
        return written;
    }

    public void ProcessPlayers()
    {
        // ⚠️ ห้ามวน _players ทั้งที่ถือ _lock อยู่ — งานที่ค้างใน player.Process() ยิง Send/Broadcast ได้
        // ถ้า Send ล้ม (บัฟเฟอร์ส่งเต็มเพราะ client ไม่อ่าน) connection จะ Close ตัวเอง
        // → ConnetionClosed → RemovePlayer → แก้ list ระหว่างที่ foreach เดินอยู่ (lock เป็น reentrant)
        // → InvalidOperationException หลุดขึ้นไปฆ่า main loop ทั้งเซิร์ฟ
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        for (int i = 0; i < snapshot.Length; i++)
        {
            snapshot[i].Process();
        }
        Animals.Process();     // เฟส C
    }
}
