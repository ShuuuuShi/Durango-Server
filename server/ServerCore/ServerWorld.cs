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

public partial class ServerWorld
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

    /// <summary>วัสดุที่ฝากไว้ในสิ่งปลูกสร้าง (entity id → slot id → รายการไอเทม)</summary>
    private readonly Dictionary<string, Dictionary<string, List<Item>>> _artifactMaterials = new Dictionary<string, Dictionary<string, List<Item>>>();
    private readonly object _artifactMaterialsLock = new object();

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

    /// <summary>สภาพอากาศ authoritative ของเกาะนี้</summary>
    public ServerWeather Weather { get; }

    /// <summary>รอยแยก/วาร์ปเรกเซเลอเรเตอร์ — state machine ของกิจกรรมป้องกันคลื่นสัตว์ (ดู WarpAcceleratorManager)</summary>
    public WarpAcceleratorManager WarpAccelerators { get; }

    /// <summary>ที่ดินส่วนตัวบนเกาะนี้</summary>
    public EstateManager Estates { get; }

    public ServerWorld(TerrainStore terrain, string serverName)
    {
        Terrain = terrain;
        ServerName = serverName;
        Animals = new AnimalSpawner(this);
        Weather = new ServerWeather(this);
        WarpAccelerators = new WarpAcceleratorManager(this);
        Estates = new EstateManager(this);
    }

    // จุดเกิด = entry point ของ terrain (1 tile = 200 หน่วย client)
    public WorldPosition GetEntryPosition()
    {
        return new WorldPosition(EntryPoint.x * 200f, EntryPoint.y * 200f);
    }

    // ผู้เล่นใหม่เข้ามา: ส่งสิ่งปลูกสร้าง + AppearPlayer ของคนเก่าให้คนใหม่ แล้ว broadcast ตัวคนใหม่ให้คนอื่น
    public void AddPlayer(ServerPlayer player)
    {
        // GP-04: สิ่งปลูกสร้างที่มีอยู่แล้ว — ทำนอก _lock เพราะใช้ล็อกคนละตัว และไม่ต้องถือ _lock ตอนทำ I/O
        AppearArtifact[] artifacts = SnapshotArtifacts();
        ServerAnimal[] animals = Animals.Snapshot();

        ServerPlayer[] others;
        lock (_lock)
        {
            others = _players.ToArray();
            _players.Add(player);
        }

        // ส่งเฉพาะสิ่งที่อยู่ในระยะมองเห็น (ที่เหลือ TickVisibility จะทยอยส่งตอนเดินไปถึง)
        // เดิมส่งทั้งเกาะให้ทุกคนที่เข้ามา — ที่ 100 คนคือ ~4,000 AppearArtifact ในชุดเดียว
        player.SendInitialVision(others, animals, artifacts);
        Weather.SendCurrent(player);

        // ให้คนที่อยู่ในระยะเห็นคนใหม่ทันที (คนไกลจะเห็นเองตอน TickVisibility รอบถัดไป)
        AnnouncePlayer(player);
        Console.WriteLine($"[world] player joined: {player.EntityId} ({player.Name}), total={Count}, artifacts={artifacts.Length}, สัตว์={Animals.Count}, client={player.ClientDescription}");
        // [3 ก.ย. 2026] มือถือของแท้ไม่มีตัวเลขคนออนไลน์บนแท็บแชทแบบ PC ชุดเรา ⇒ เซิร์ฟบอกให้เอง
        NotifyOnlineCount(player, joined: true);
        // [แก้เอง] ระบบ mod: แจ้ง mod ที่ลงทะเบียน OnPlayerJoined ไว้
        PluginManager.Instance?.FirePlayerJoined(player);
    }

    public void RemovePlayer(ServerPlayer player)
    {
        lock (_lock)
        {
            _players.Remove(player);
        }
        AnnounceGone(player.EntityId);
        Console.WriteLine($"[world] player left: {player.EntityId} ({player.Name}), total={Count}");
        NotifyOnlineCount(player, joined: false);
        // [แก้เอง] ระบบ mod: แจ้ง mod ที่ลงทะเบียน OnPlayerLeft ไว้
        PluginManager.Instance?.FirePlayerLeft(player);
    }

    /// <summary>
    /// [3 ก.ย. 2026] "ระบบเหมือน PC" ให้มือถือ (ServerConfig.Android):
    ///   · คนที่เพิ่งเข้า (ถ้าเป็นมือถือ) ได้ popup ยินดีต้อนรับ + จำนวนคนออนไลน์
    ///   · มือถือคนอื่น ๆ ได้บรรทัดแชทช่อง System "X เข้าเกม/ออกจากเกม · ออนไลน์ N คน"
    /// PC ชุดเราไม่ได้รับ (มีตัวเลขบนแท็บแชทจาก /knock อยู่แล้ว — ChattingTabList) จะได้ไม่ซ้ำ
    /// </summary>
    private void NotifyOnlineCount(ServerPlayer who, bool joined)
    {
        AndroidConfig cfg = ServerConfig.Current?.Android;
        if (cfg == null) return;
        int online = Count;
        if (joined && cfg.WelcomeInfo && who.WantsServerSideOnlineCount)
        {
            // เกมของแท้ไม่แสดง Info ⇒ ใช้ RadioNotice (popup + บรรทัดในแท็บ "ระบบ")
            who.SendNotice($"ยินดีต้อนรับสู่ {ServerName} · ออนไลน์ตอนนี้ {online} คน");
        }
        if (!cfg.OnlineCountInChat) return;
        string name = string.IsNullOrEmpty(who.Name) ? "ผู้เล่น" : who.Name;
        string text = joined
            ? $"{name} เข้าเกม · ออนไลน์ {online} คน"
            : $"{name} ออกจากเกม · ออนไลน์ {online} คน";
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        foreach (ServerPlayer p in snapshot)
        {
            if (p == who || !p.WantsServerSideOnlineCount) continue;
            p.SendSystemChat(text);
        }
    }

    /// <summary>
    /// [3 ก.ย. 2026] บรอดแคสต์ Info (popup) ให้ทุกคน — ข้อความแบบมีสไตล์ "##bc|d=|z=|c=|ข้อความ"
    /// ส่งเต็ม ๆ เฉพาะ client ที่รู้จัก (CustomClient ≥ StyledBroadcastMinClientVersion)
    /// ที่เหลือ (มือถือของแท้ / PC รุ่นเก่า) ได้เฉพาะข้อความ ไม่งั้นเห็นรหัสดิบบนจอ
    /// </summary>
    public void BroadcastInfo(string payload)
    {
        string plain = ClientPlatform.PlainBroadcastText(payload);
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        foreach (ServerPlayer p in snapshot)
        {
            if (p.IsAndroid)
            {
                // [4 ก.ย. 2026] เกมมือถือของแท้ไม่แสดง Info เลย ⇒ ส่งเป็น RadioNotice (popup ของเกมเอง)
                p.SendNotice(plain);
                continue;
            }
            p.Send(new Info { Text = p.SupportsStyledBroadcast ? payload : plain });
        }
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
    /// <summary>
    /// พื้นที่ที่จะสร้าง **ทับของเดิมไหม** — เช็คทุก tile ที่ของใหม่จะกิน ไม่ใช่แค่ tile มุม
    ///
    /// 🐛 เดิมใช้ <see cref="HasArtifactAt"/> ซึ่งเช็คแค่ **tile เดียว (มุมของของใหม่)**
    /// ⇒ ของ 2×2 วางเยื้องไป 1 ช่องจะ "ผ่าน" ทั้งที่ทับของเดิมอยู่ครึ่งหนึ่ง
    /// (มุมว่างก็พอแล้ว) — วางบ้านซ้อนกันได้จริงในเกม
    /// </summary>
    /// <param name="ignoreEntityId">ข้ามชิ้นนี้ตอนตรวจ — ใช้ตอน "ย้ายที่" ไม่งั้นมันชนตัวเอง</param>
    public bool HasArtifactOverlapping(Point2 tile, Point2 size, string ignoreEntityId = null)
    {
        int nx = size.x <= 0 ? 1 : size.x;
        int ny = size.y <= 0 ? 1 : size.y;
        lock (_artifactLock)
        {
            foreach (AppearArtifact a in _artifacts.Values)
            {
                if (ignoreEntityId != null && a.EntityId == ignoreEntityId)
                {
                    continue;
                }
                int sx = a.Size.x <= 0 ? 1 : a.Size.x;
                int sy = a.Size.y <= 0 ? 1 : a.Size.y;
                // สองสี่เหลี่ยมทับกันเมื่อ "ไม่ได้แยกกันในแกนไหนเลย"
                bool apart = tile.x + nx <= a.Tile.x || a.Tile.x + sx <= tile.x
                          || tile.y + ny <= a.Tile.y || a.Tile.y + sy <= tile.y;
                if (!apart)
                {
                    return true;
                }
            }
        }
        return false;
    }

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

    /// <summary>
    /// ส่งให้ **เฉพาะคนที่อยู่ในระยะมองเห็นของจุดนี้** (interest management)
    ///
    /// ใช้แทน <see cref="Broadcast{T}"/> กับทุกอย่างที่ "เกิดขึ้นที่ใดที่หนึ่งในโลก" —
    /// คนเดิน · สัตว์เดิน · ดาเมจ · สิ่งปลูกสร้าง · หลอดเลือด
    /// (แชท/ประกาศยังต้องใช้ <c>Broadcast</c> เพราะไม่ได้ผูกกับตำแหน่ง)
    ///
    /// ระยะที่ใช้คือ **ระยะหาย** (ViewExitUnits) ไม่ใช่ระยะเริ่มเห็น —
    /// ของที่คนหนึ่งเห็นอยู่แล้วต้องอัปเดตต่อจนกว่าจะหลุดระยะหายจริง ๆ ไม่งั้นจะค้างกลางอากาศ
    /// </summary>
    public void BroadcastNear<T>(WorldPosition at, T msg, ServerPlayer except = null) where T : struct
    {
        WorldConfig cfg = ServerConfig.Current.World;
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        if (!cfg.ViewCulling)
        {
            // ปิดการกรองระยะ = พฤติกรรมเดิมเป๊ะ
            foreach (ServerPlayer p in snapshot)
            {
                if (p != except)
                {
                    p.Send(msg);
                }
            }
            return;
        }
        float limit = cfg.ViewExitUnits;
        float limitSq = limit * limit;
        foreach (ServerPlayer p in snapshot)
        {
            if (p == except)
            {
                continue;
            }
            WorldPosition me = p.CurrentPosition;
            float dx = me.x - at.x;
            float dy = me.y - at.y;
            if (dx * dx + dy * dy <= limitSq)
            {
                p.Send(msg);
            }
        }
    }

    /// <summary>
    /// ส่งข่าว **เกี่ยวกับ entity ตัวหนึ่ง** ให้เฉพาะคนที่กำลังมองเห็นมันอยู่
    /// (เดิน · โดนตี · ตาย · เปลี่ยนชุด · หลอดเลือด)
    ///
    /// ถูกต้องกว่าการวัดระยะดิบ ๆ เพราะคนที่ยังไม่ได้รับ Appear ของ entity นี้
    /// **ไม่ควรได้รับข่าวของมันเลย** — client จะได้ Move ของ entity ที่ตัวเองไม่รู้จัก
    /// </summary>
    public void BroadcastToViewers<T>(string entityId, T msg, ServerPlayer except = null) where T : struct
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return;
        }
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        foreach (ServerPlayer p in snapshot)
        {
            if (p != except && p.CanSee(entityId))
            {
                p.Send(msg);
            }
        }
    }

    /// <summary>
    /// entity เกิดใหม่ในโลก — ให้คนที่อยู่ในระยะเห็นทันที ไม่ต้องรอรอบตรวจถัดไป
    ///
    /// ⚠️ ต้องผ่าน <c>Observe*</c> ของแต่ละคน ห้าม broadcast <c>MakeAppear()</c> ตรง ๆ
    /// ไม่งั้นรอบตรวจถัดไปจะเห็นว่า id นี้ยังไม่อยู่ในเซ็ตแล้ว **ส่ง Appear ซ้ำอีกที**
    /// </summary>
    public void AnnounceAnimal(ServerAnimal animal)
    {
        if (animal == null)
        {
            return;
        }
        ForEachNear(animal.Position, p => p.ObserveAnimal(animal));
    }

    public void AnnounceArtifact(AppearArtifact artifact)
    {
        WorldPosition at = new WorldPosition(artifact.Tile.x * 200f + 100f, artifact.Tile.y * 200f + 100f);
        ForEachNear(at, p => p.ObserveArtifact(artifact));
    }

    public void AnnouncePlayer(ServerPlayer player)
    {
        ForEachNear(player.CurrentPosition, p =>
        {
            if (p != player)
            {
                p.ObservePlayer(player);
            }
        });
    }

    /// <summary>entity ถูกลบออกจากโลก — บอกทุกคนให้ลบทิ้ง แล้วล้างออกจากเซ็ต "ที่เห็นอยู่" ของทุกคน</summary>
    public void AnnounceGone(string entityId)
    {
        if (string.IsNullOrEmpty(entityId))
        {
            return;
        }
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        foreach (ServerPlayer p in snapshot)
        {
            // ส่งให้ทุกคนไม่ว่าอยู่ใกล้ไหม — ถ้าเขาเคยเห็นแล้วเดินหนีไป client อาจยังถือ entity ค้างอยู่
            // packet เล็กและเหตุการณ์นี้ไม่ถี่ จึงไม่คุ้มที่จะไปไล่เช็คว่าใครเคยเห็นบ้าง
            p.Send(new DisappearEntity { EntityId = entityId });
            p.ForgetEntity(entityId);
        }
    }

    /// <summary>วนเฉพาะคนที่อยู่ในระยะมองเห็นของจุดหนึ่ง</summary>
    private void ForEachNear(WorldPosition at, Action<ServerPlayer> action)
    {
        WorldConfig cfg = ServerConfig.Current.World;
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        bool culling = cfg.ViewCulling;
        float limitSq = cfg.ViewEnterUnits * cfg.ViewEnterUnits;
        foreach (ServerPlayer p in snapshot)
        {
            if (!culling)
            {
                action(p);
                continue;
            }
            WorldPosition me = p.CurrentPosition;
            float dx = me.x - at.x;
            float dy = me.y - at.y;
            if (dx * dx + dy * dy <= limitSq)
            {
                action(p);
            }
        }
    }

    /// <summary>
    /// เรียกทุก tick — ตรวจว่าใคร/ตัวไหนเพิ่งเข้ามาในระยะหรือหลุดออกไป
    /// แล้วส่ง Appear/Disappear ให้ถูกจังหวะ (ดู ServerPlayer.Vision)
    ///
    /// ต้องมีตัวนี้ เพราะการกรองระยะตอน broadcast อย่างเดียวทำให้คนที่เดินเข้ามาใหม่
    /// **ไม่มีวันรู้ว่ามีใครอยู่ตรงนั้น** — เขาได้แต่ packet ของคนที่ตัวเองรู้จักอยู่แล้ว
    /// </summary>
    public void TickVisibility(double now)
    {
        if (!ServerConfig.Current.World.ViewCulling || now < _nextVisibilityTickAt)
        {
            return;
        }
        _nextVisibilityTickAt = now + ServerConfig.Current.World.ViewCheckSeconds;
        ServerPlayer[] snapshot;
        lock (_lock)
        {
            snapshot = _players.ToArray();
        }
        ServerAnimal[] animals = Animals.Snapshot();
        AppearArtifact[] artifacts = SnapshotArtifacts();
        for (int i = 0; i < snapshot.Length; i++)
        {
            try
            {
                snapshot[i].TickVisibility(snapshot, animals, artifacts, now);
            }
            catch (Exception e)
            {
                Console.WriteLine("[vision] {0}: {1}", snapshot[i].Name, e.Message);
            }
        }
    }

    private double _nextVisibilityTickAt;

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

    /// <summary>
    /// สิ่งปลูกสร้างชนิดนี้ใช้พักผ่อนได้ไหม — ต้องใช้ component `Shelter` จากข้อมูลเกมจริง
    /// เช่นเดียวกับเกณฑ์ที่ client ใช้เพิ่ม Interaction.Rest ใน Player.cs
    /// ห้ามเดาจากชื่อ blueprint เพราะเก้าอี้/โซฟา/เสื่อ/สระน้ำจำนวนมากใช้พักได้แต่ชื่อไม่มีคำว่า
    /// fire/tent/bed/rest ขณะที่ของประดับบางชิ้นมีคำเหล่านี้แต่ไม่ใช่ที่พัก
    /// </summary>
    private static bool IsRestBlueprint(string blueprintId)
    {
        if (string.IsNullOrEmpty(blueprintId)
            || !RecipeData.BlueprintComponents.TryGetValue(blueprintId, out string[] components)
            || components == null)
        {
            return false;
        }
        for (int i = 0; i < components.Length; i++)
        {
            if (string.Equals(components[i], "Shelter", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
        return false;
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
        // วัสดุที่ฝากไว้ — caller ควร TakeArtifactMaterials ก่อนเรียกเพื่อ refund
        // แต่ถ้าไม่ได้ take ก็ต้อง cleanup ตรงนี้กัน leak
        lock (_artifactMaterialsLock)
        {
            _artifactMaterials.Remove(entityId ?? string.Empty);
        }
        // ทุบแปลงผัก = ต้นที่ปลูกไว้หายไปด้วย (ไม่งั้นแปลงค้างในตารางตลอดกาล)
        lock (_farmLock)
        {
            _farms.Remove(entityId ?? string.Empty);
        }
        lock (_genLock)
        {
            _generators.Remove(entityId ?? string.Empty);
        }
        if (removed)
        {
            MarkDirty();
        }
        return removed;
    }

    /// <summary>
    /// ย้ายสิ่งปลูกสร้าง/POI ไป tile ใหม่ตอนเซิร์ฟกำลังรัน (ใช้จาก `cheat poi move`)
    ///
    /// ทำไมต้องมี: เดิมแก้ตำแหน่ง POI ได้ทางเดียวคือหยุดเซิร์ฟ → แก้ world.json มือ → เปิดใหม่
    /// ซึ่งช้ามากตอนไล่หาว่า "หลุมควรอยู่ตรงไหนถึงจะไม่โดนหินทับ"
    ///
    /// เคลียร์ของธรรมชาติใต้ที่ใหม่ให้ด้วย ไม่งั้นย้ายไปโดนหินบังเหมือนเดิม
    /// </summary>
    public bool MoveArtifact(string entityId, Point2 tile)
    {
        AppearArtifact moved;
        lock (_artifactLock)
        {
            if (!_artifacts.TryGetValue(entityId ?? string.Empty, out AppearArtifact a))
            {
                return false;
            }
            a.Tile = tile;
            _artifacts[entityId] = a;
            moved = a;
        }
        int sx = moved.Size.x <= 0 ? 1 : moved.Size.x;
        int sy = moved.Size.y <= 0 ? 1 : moved.Size.y;
        for (int x = 0; x < sx; x++)
        {
            for (int y = 0; y < sy; y++)
            {
                Terrain.RemoveNatural(tile.x + x, tile.y + y);
            }
        }
        // client จำตำแหน่งเดิมไว้ — ต้องสั่งลบก่อนแล้วค่อยประกาศตัวใหม่
        // ไม่งั้นในจอผู้เล่นจะเห็นของชิ้นเดียวกันสองที่
        AnnounceGone(entityId);
        AnnounceArtifact(moved);
        MarkDirty();
        return true;
    }

    /// <summary>เพิ่มผู้ร่วมแก้ไข artifact สำหรับการทดสอบสิทธิ์หลายผู้เล่น</summary>
    public bool TryAddArtifactArchitect(string entityId, string architectEntityId, string founderEntityId, out AppearArtifact updated)
    {
        updated = default;
        if (string.IsNullOrWhiteSpace(entityId) || string.IsNullOrWhiteSpace(architectEntityId))
        {
            return false;
        }
        lock (_artifactLock)
        {
            if (!_artifacts.TryGetValue(entityId, out AppearArtifact artifact)
                || !string.Equals(artifact.FounderEntityId, founderEntityId, StringComparison.Ordinal))
            {
                return false;
            }
            var architects = new List<string>(artifact.ArchitectEntityIds ?? Array.Empty<string>());
            bool exists = false;
            for (int i = 0; i < architects.Count; i++)
            {
                if (string.Equals(architects[i], architectEntityId, StringComparison.Ordinal))
                {
                    exists = true;
                    break;
                }
            }
            if (!exists)
            {
                architects.Add(architectEntityId);
                artifact.ArchitectEntityIds = architects.ToArray();
                _artifacts[entityId] = artifact;
                MarkDirty();
            }
            updated = artifact;
            return true;
        }
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

    /// <summary>
    /// อัปเดตสถานะกิจกรรม "รอยแยก/วาร์ปเรกเซเลอเรเตอร์" ของ artifact ชิ้นหนึ่ง แล้ว broadcast ให้คนที่เห็นอยู่รู้ทันที
    ///
    /// ใช้ทางเดียวกับ <see cref="MoveArtifact"/> คือส่ง <see cref="AppearArtifact"/> เต็มใบซ้ำผ่าน
    /// <see cref="AnnounceArtifact"/> — เพราะฝั่ง client (ArtifactManager.OnAppearArtifactMsg →
    /// Artifact.SetArtifactState → event ArtifactStateChanged) รับรู้การเปลี่ยน ArtifactState จาก
    /// AppearArtifact เท่านั้น ไม่มี message เฉพาะสำหรับอัปเดตบางส่วน (WarpAcceleratorInfo/…ในเกม
    /// เป็นแค่ cache ฝั่ง client ที่คำนวณต่อจาก event นี้อีกที ไม่ใช่สิ่งที่ server ต้อง push เอง)
    ///
    /// ⚠️ ไม่เรียก MarkDirty() — สถานะนี้ไม่ได้ถูกเซฟ (ArtifactSave ไม่มีฟิลด์ Warpaccelerator เหมือนที่
    /// AnimalSpawner ตั้งใจไม่เซฟสัตว์) และ WarpAcceleratorManager.Process() เรียกฟังก์ชันนี้ได้บ่อยมาก
    /// (ทุก tick ที่สถานะเปลี่ยน) — เรียก MarkDirty ทุกครั้งจะยิง disk I/O ถี่เกินจำเป็นโดยไม่มีประโยชน์
    /// </summary>
    /// <summary>
    /// [4 ก.ย. 2026] เติมโมเดล (Display.Parts) ของสิ่งปลูกสร้างที่เพิ่งสร้างเสร็จ แล้ว re-announce
    /// (ไซต์ถูกส่งด้วย Parts เปล่า — ดู ArtifactFactory.Make ⇒ ต้องเติมตอนเสร็จ ไม่งั้นสร้างเสร็จแล้วมองไม่เห็น)
    /// </summary>
    public void RefreshArtifactDisplayParts(string entityId, string blueprintId)
    {
        AppearArtifact updated;
        lock (_artifactLock)
        {
            if (!_artifacts.TryGetValue(entityId ?? string.Empty, out AppearArtifact a))
            {
                return;
            }
            ArtifactDisplay display = a.Display;
            display.Parts = ArtifactFactory.BuildParts(blueprintId);
            a.Display = display;
            _artifacts[entityId] = a;
            updated = a;
        }
        AnnounceArtifact(updated);
        MarkDirty();
    }

    public void SetArtifactWarpAccelerator(string entityId, Messages.WarpAccelerator state)
    {
        AppearArtifact updated;
        lock (_artifactLock)
        {
            if (!_artifacts.TryGetValue(entityId ?? string.Empty, out AppearArtifact a))
            {
                return;
            }
            ArtifactState states = a.States;
            states.Warpaccelerator = state;
            a.States = states;
            _artifacts[entityId] = a;
            updated = a;
        }
        AnnounceArtifact(updated);
    }

    // ---------------------------------------------------------------- GP-07: เซฟ/โหลดโลก

    /// <summary>เขียนสิ่งปลูกสร้าง + ต้นไม้ที่ถูกเก็บไปแล้ว ลงดิสก์</summary>
    public bool Save()
    {
        WorldSave save = new WorldSave
        {
            TerrainId = Terrain.TerrainId,
            RemovedNaturals = Terrain.GetRemovedNaturals(),
            RegrowableNaturals = Terrain.GetRegrowableNaturals()
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

        // วัสดุที่ฝากไว้ในสิ่งปลูกสร้าง
        foreach (var kv in SnapshotArtifactMaterials())
        {
            var slots = new Dictionary<string, List<ItemSave>>();
            foreach (var slot in kv.Value)
            {
                var list = new List<ItemSave>(slot.Value.Count);
                for (int i = 0; i < slot.Value.Count; i++)
                {
                    list.Add(ItemSave.From(slot.Value[i]));
                }
                slots[slot.Key] = list;
            }
            save.ArtifactMaterials[kv.Key] = slots;
        }

        save.Estates = Estates.ToSave();

        // แปลงผัก — เก็บ "จำนวนที่เหลือจริง" จาก generator ไปด้วย ไม่งั้นรีสตาร์ทแล้วผลผลิตเกิดใหม่
        FarmPlot[] plots = SnapshotFarms();
        for (int i = 0; i < plots.Length; i++)
        {
            FarmPlot p = plots[i];
            int product = 0, seed = 0;
            if (CropData.TryGet(p.SeedId, out CropData.CropInfo crop))
            {
                Generator[] gens = PeekGenerators(p.ArtifactId);
                if (gens != null)
                {
                    for (int g = 0; g < gens.Length; g++)
                    {
                        if (gens[g].Id == crop.ProductId)
                        {
                            product = gens[g].Amount;
                        }
                        else if (gens[g].Id == crop.SeedProductId)
                        {
                            seed = gens[g].Amount;
                        }
                    }
                }
            }
            save.Farms.Add(FarmSave.From(p, product, seed));
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
        string worldPath = SaveStore.WorldPath;
        bool hadSave = File.Exists(worldPath) || File.Exists(worldPath + ".tmp");
        WorldSave save = SaveStore.Load<WorldSave>(worldPath);
        if (save == null)
        {
            if (hadSave)
            {
                throw new InvalidDataException($"ไฟล์เซฟโลกอ่านไม่ได้หรือ JSON เสียและถูกกักกัน: {worldPath}");
            }
            Console.WriteLine("[save] ยังไม่มีไฟล์เซฟโลก — เริ่มจากแมพเปล่า");
            EnsureNaturalPOIs();
            return;
        }
        if (!string.IsNullOrEmpty(save.TerrainId) && save.TerrainId != Terrain.TerrainId)
        {
            Console.WriteLine($"[save] ⚠️ เซฟเป็นแมพ '{save.TerrainId}' แต่ตอนนี้โหลดแมพ '{Terrain.TerrainId}' — ตำแหน่งของอาจเพี้ยน");
        }

        int naturals = Terrain.ApplyRemovedNaturals(save.RemovedNaturals ?? new List<int[]>());
        Terrain.ApplyRegrowableNaturals(save.RegrowableNaturals);
        save.Artifacts ??= new List<ArtifactSave>();
        save.Farms ??= new List<FarmSave>();
        save.Boxes ??= new Dictionary<string, List<ItemSave>>();

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

        // วัสดุที่ฝากไว้ในสิ่งปลูกสร้าง
        int depositedSlots = 0;
        if (save.ArtifactMaterials != null)
        {
            foreach (var kv in save.ArtifactMaterials)
            {
                if (kv.Value == null || kv.Value.Count == 0) continue;
                var slots = new Dictionary<string, List<Item>>();
                foreach (var slot in kv.Value)
                {
                    if (slot.Value == null || slot.Value.Count == 0) continue;
                    var items = new List<Item>(slot.Value.Count);
                    for (int i = 0; i < slot.Value.Count; i++)
                    {
                        if (slot.Value[i] != null && !string.IsNullOrEmpty(slot.Value[i].Id))
                        {
                            items.Add(slot.Value[i].ToItem());
                        }
                    }
                    if (items.Count > 0) slots[slot.Key] = items;
                }
                if (slots.Count > 0)
                {
                    RestoreArtifactMaterials(kv.Key, slots);
                    depositedSlots += slots.Count;
                }
            }
        }

        // แปลงผัก — ต้องหลัง artifact เพราะ ApplyFarmToArtifact ต้องเจอ artifact ในตารางแล้ว
        LoadFarms(save.Farms);
        Estates.Load(save.Estates);

        _dirty = false;
        Console.WriteLine($"[save] โหลดโลกแล้ว: สิ่งปลูกสร้าง {loaded} ชิ้น, ธรรมชาติที่ถูกเก็บไปแล้ว {naturals} จุด, กล่องที่มีของ {boxes} ใบ, วัสดุฝาก {depositedSlots} slots, แปลงผัก {FarmCount} แปลง, ที่ดิน {Estates.ToSave().Count} แปลง");
        EnsureNaturalPOIs();
    }

    /// <summary>
    /// วาง POI ธรรมชาติ (รอยแยก/หลุมวาร์ป/ท่าเรือ) ลงบนเกาะครั้งเดียวตอนเริ่มโลก
    ///
    /// 🐛 เดิมเกาะว่างเปล่า — POI ทั้งหมดมาจาก artifact ที่ผู้เล่นสร้างเองเท่านั้น
    /// ⇒ ผู้เล่นใหม่ไม่เห็นรอยแยก (crack) หรือหลุมวาร์ปเลย แผนที่โลกแถบว่าง
    ///
    /// วางเป็น artifact ปกติ (blueprint warp_accelerator/camp_warphole/neutral_warphole/dock)
    /// → client วาดเป็นวัตถุจริงบนเกาะ + POIUpdater เจอเป็น POI (ค้นหา/วาร์ป/แผนที่) + บันทึกในเซฟโลก
    ///
    /// 🐛 เดิมสุ่มทั้งแผนที่ + ห่างจากจุดเกิด 20-30 tile ⇒ POI ทั้งหมดอาจไปตกอยู่ฝั่งไกล
    /// (โลกนี้ entry=(40,177) แต่ POI ตก (115,52)-(205,131) — ห่าง ~150 tile) ⇒ กดสแกนหลุม
    /// (รัศมี 50 tile) เจอ 0 จุดเสมอ — จึงแบ่งเป็น 2 ชุด: ชุดใกล้จุดเกิด + ชุดกระจายทั่วเกาะ
    /// </summary>
    private void EnsureNaturalPOIs()
    {
        // 🐛 หลุมวาร์ปโดนหินทับ — ตัวที่วางรอบนี้ถูกเคลียร์ให้แล้วใน PlacePOISpots
        // แต่ **ตัวที่อยู่ใน world.json มาก่อนหน้านั้นไม่เคยถูกเคลียร์** (เจอจริง 4 ชิ้น
        // มีต้นไม้/หินทับ 2-6 จุด) เพราะโค้ดเคลียร์ทำงานตอน "วางใหม่" เท่านั้น
        // ⇒ กวาดซ้ำทุกครั้งที่เปิดเซิร์ฟ โลกเก่าจะได้ซ่อมตัวเอง
        bool cleared = ClearNaturalsUnderPOIs();

        // [แก้เอง] 2 ก.ย. 2026 — ใช้พิกัดที่ "มากับเกาะ" ก่อนเสมอ
        //
        // เกาะของเกมทุกใบมี pois.yml ที่ทีมสร้างเกมวางตำแหน่งไว้ให้แล้ว (หลุมวาร์ป 5 · รอยแยก 2
        // · ท่าเรือ 1 · สิ่งปลูกสร้าง 1) ตรวจแล้วว่าไม่มีจุดไหนตกน้ำหรือทับหินเลยสักจุด
        // และ port_points ตรงกับ entry_points เป๊ะ — แต่เดิมเซิร์ฟไม่เคยอ่านไฟล์นี้
        // เลยไปสุ่มตำแหน่งเองทั้งหมด ซึ่งเป็นที่มาของอาการ POI ลอยกลางน้ำ/โดนหินทับ
        bool placed = PlaceIslandPois();

        // เกาะที่ไม่มี pois.yml (เช่นเกาะที่ปั่นเอง) ค่อยถอยไปสุ่มแบบเดิม
        if (Terrain.Pois == null)
        {
            placed |= EnsureNearEntryPOIs();
            placed |= PlacePOISpots(spots: new[]
            {
                // ท่าเรือที่ 2 — ติดแม่น้ำ ห่างจากจุดเกิด 40+ tile (ท่าเรือรองสำหรับเดินทาง)
                ("dock", (ushort)7001, new Point2(3, 3), 0, 40, true),
            }, prefix: "poi_", nearEntry: false);
        }
        if (placed || cleared)
        {
            MarkDirty();
        }
    }

    /// <summary>
    /// เอาต้นไม้/หินที่ทับตัว POI ออก — เดินเข้าไปใช้งานไม่ได้ถ้ามีของบัง
    ///
    /// ทำกับ POI ที่มีอยู่แล้วในเซฟด้วย ไม่ใช่เฉพาะตอนวางใหม่
    /// (ของธรรมชาติมาจาก natural layer ของ terrain ซึ่งไม่รู้เรื่อง artifact ที่วางทับ)
    /// </summary>
    private bool ClearNaturalsUnderPOIs()
    {
        AppearArtifact[] all = SnapshotArtifacts();
        int cleared = 0;
        for (int i = 0; i < all.Length; i++)
        {
            AppearArtifact a = all[i];
            if (!TryGetArtifactBlueprint(a.EntityId, out string bp) || bp == null)
            {
                continue;
            }
            if (!IsPOIBlueprint(bp))
            {
                continue;
            }
            int sx = a.Size.x <= 0 ? 1 : a.Size.x;
            int sy = a.Size.y <= 0 ? 1 : a.Size.y;
            int here = 0;
            for (int x = 0; x < sx; x++)
            {
                for (int y = 0; y < sy; y++)
                {
                    if (Terrain.RemoveNatural(a.Tile.x + x, a.Tile.y + y))
                    {
                        here++;
                    }
                }
            }
            if (here > 0)
            {
                Console.WriteLine("[world] เคลียร์ต้นไม้/หิน {0} จุดที่ทับ {1} (tile {2},{3})",
                    here, a.EntityId, a.Tile.x, a.Tile.y);
                cleared += here;
            }
        }
        return cleared > 0;
    }

    /// <summary>blueprint นี้เป็นจุดสนใจ (ท่าเรือ/หลุมวาร์ป) ไหม</summary>
    private static bool IsPOIBlueprint(string blueprintId)
    {
        switch (blueprintId)
        {
            case "dock":
            case "camp_warphole":
            case "neutral_warphole":
            case "cargo_warphole_in":
            case "warp_accelerator":
            case "warphole_personal":
                return true;
            default:
                return false;
        }
    }

    /// <summary>
    /// วาง POI ตามพิกัดที่มากับเกาะ (`pois.yml`) — id ขึ้นต้น `poi_island_`
    ///
    /// ชนิดที่จับคู่ไว้ (entity type จาก RecipeData):
    ///   port_points    -> dock 7001 (3x3)
    ///   warpholes      -> neutral_warphole 9450 (6x6)
    ///   rifts          -> warp_accelerator 6282 (4x4) — ArtifactFactory เรียกสถานะนี้ว่า RiftInactivated
    ///   camp_artifacts -> ใช้ entity_type ที่ไฟล์ระบุมาเอง (ri35te = 9450)
    ///
    /// พิกัดในไฟล์คือมุมของ footprint ถ้าวางตรงนั้นแล้วล้นลงน้ำจะขยับหาที่ใกล้ ๆ ให้เอง
    /// </summary>
    private bool PlaceIslandPois()
    {
        TerrainPois? pois = Terrain.Pois;
        if (pois == null)
        {
            return false;
        }

        // ⚠️ id ต้อง **คงที่** ผูกกับลำดับในไฟล์ ไม่ใช่ไล่เลขตอนวาง
        //    ไม่งั้นเปิดเซิร์ฟรอบสองจะเห็นว่า id เดิมถูกใช้แล้วเลยตั้งเลขใหม่ แล้ววางซ้ำอีกชุด
        //    (เจอจริง: เปิด 3 รอบได้ POI 28 จุดจากที่ควรมี 9)
        var wanted = new List<(string Key, string Bp, ushort Type, Point2 Size, Point2 Tile)>();
        for (int i = 0; i < pois.PortPoints.Count; i++)
        {
            wanted.Add(("port_" + i, "dock", (ushort)7001, new Point2(3, 3), pois.PortPoints[i]));
        }
        for (int i = 0; i < pois.Warpholes.Count; i++)
        {
            wanted.Add(("warphole_" + i, "neutral_warphole", (ushort)9450, new Point2(6, 6), pois.Warpholes[i]));
        }
        for (int i = 0; i < pois.Rifts.Count; i++)
        {
            wanted.Add(("rift_" + i, "warp_accelerator", (ushort)6282, new Point2(4, 4), pois.Rifts[i]));
        }
        for (int i = 0; i < pois.CampArtifacts.Count; i++)
        {
            (ushort type, Point2 tile) = pois.CampArtifacts[i];
            // ไฟล์บอก entity_type มาเอง — แปลงกลับเป็น blueprint จากตารางที่ cheat poi ใช้อยู่
            string bp = "neutral_warphole";
            var size = new Point2(6, 6);
            foreach (KeyValuePair<string, (ushort Type, int SizeX, int SizeY)> kv in ServerPlayer.POIBlueprints)
            {
                if (kv.Value.Type == type)
                {
                    bp = kv.Key;
                    size = new Point2(kv.Value.SizeX, kv.Value.SizeY);
                    break;
                }
            }
            wanted.Add(("camp_" + i, bp, type, size, tile));
        }

        // โลกเก่าที่เคยเปิดด้วยระบบสุ่มมี POI อยู่แล้ว (id `poi_` / `poi_near_`) ถ้าไม่เอาออก
        // จะได้ POI ซ้อนกันสองชุด — ชุดที่สุ่มมั่วกับชุดที่ถูกต้อง
        // ⇒ ย้ายมาใช้ชุดของเกมแทน แต่สำรอง world.json ไว้ก่อนเสมอ (ย้อนกลับได้ถ้าไม่ถูกใจ)
        var stale = new List<string>();
        lock (_artifactLock)
        {
            foreach (string id in _artifacts.Keys)
            {
                if ((id.StartsWith("poi_", StringComparison.Ordinal)
                     || id.StartsWith("poi_near_", StringComparison.Ordinal))
                    && !id.StartsWith("poi_island_", StringComparison.Ordinal))
                {
                    stale.Add(id);
                }
            }
        }
        if (stale.Count > 0)
        {
            BackupWorldSave("ก่อนเปลี่ยนไปใช้ POI จาก pois.yml");
            foreach (string id in stale)
            {
                RemoveArtifact(id);
            }
            Console.WriteLine("[world] เอา POI ที่ระบบสุ่มวางไว้ออก {0} จุด แล้วใช้พิกัดจาก pois.yml แทน ({1})",
                stale.Count, string.Join(", ", stale));
        }

        var existing = new HashSet<string>(StringComparer.Ordinal);
        lock (_artifactLock)
        {
            foreach (string id in _artifacts.Keys) { existing.Add(id); }
        }

        // เก็บกวาดของที่เคยวางซ้ำจากบั๊กรอบก่อน — อะไรที่ขึ้นต้น poi_island_ แต่ไม่อยู่ในชุดที่ควรมี ให้เอาออก
        var shouldHave = new HashSet<string>(wanted.Select(x => "poi_island_" + x.Key), StringComparer.Ordinal);
        var extras = existing.Where(id => id.StartsWith("poi_island_", StringComparison.Ordinal)
                                          && !shouldHave.Contains(id)).ToList();
        if (extras.Count > 0)
        {
            BackupWorldSave("ก่อนเก็บ POI ที่วางซ้ำ");
            foreach (string id in extras)
            {
                RemoveArtifact(id);
                existing.Remove(id);
            }
            Console.WriteLine("[world] เก็บ POI ที่วางซ้ำออก {0} จุด", extras.Count);
        }

        int placed = 0;
        foreach ((string key, string bp, ushort type, Point2 size, Point2 tile) in wanted)
        {
            string entityId = "poi_island_" + key;
            if (existing.Contains(entityId))
            {
                continue;               // วางไปแล้วตั้งแต่รอบก่อน ไม่ต้องทำอะไร
            }

            if (!TryFitPoi(tile, size, out Point2 at))
            {
                Console.WriteLine("[world] POI {0} ที่ tile {1},{2} วางไม่ได้ (ไม่ใช่พื้นดินหรือมีของทับ) — ข้าม",
                    bp, tile.x, tile.y);
                continue;
            }
            existing.Add(entityId);
            for (int x = 0; x < size.x; x++)
            {
                for (int y = 0; y < size.y; y++)
                {
                    Terrain.RemoveNatural(at.x + x, at.y + y);
                }
            }
            AppearArtifact artifact = ArtifactFactory.Make(
                null, entityId, type, at, size, Rotation.None, 0, 1, bp, BuildingState.Completed);
            AddArtifact(artifact, bp);
            Console.WriteLine("[world] วาง POI จาก pois.yml: {0} (tile {1},{2})", bp, at.x, at.y);
            placed++;
        }
        if (placed > 0)
        {
            Console.WriteLine("[world] วาง POI ตามพิกัดที่มากับเกาะแล้ว {0} จุด", placed);
        }
        return placed > 0;
    }

    /// <summary>สำรองไฟล์เซฟโลกไว้ก่อนแก้อะไรที่ย้อนกลับเองไม่ได้</summary>
    private static void BackupWorldSave(string why)
    {
        try
        {
            string path = SaveStore.WorldPath;
            if (!File.Exists(path))
            {
                return;
            }
            string backup = path + ".bak-" + System.DateTime.Now.ToString("yyyyMMdd-HHmmss");
            File.Copy(path, backup, overwrite: false);
            Console.WriteLine("[save] สำรองเซฟโลกไว้ที่ {0} ({1})", Path.GetFileName(backup), why);
        }
        catch (Exception e)
        {
            Console.WriteLine("[save] สำรองเซฟโลกไม่สำเร็จ: {0}", e.Message);
        }
    }

    /// <summary>
    /// หาที่วาง POI ให้พอดี — ลองพิกัดที่ไฟล์บอกก่อน ถ้าล้นน้ำ/ทับของเดิมค่อยขยับหารอบ ๆ
    /// คืน false ถ้าไม่มีที่ว่างเลยในรัศมี 6 tile (แปลว่าไฟล์กับ terrain ไม่เข้ากันจริง ๆ)
    /// </summary>
    private bool TryFitPoi(Point2 tile, Point2 size, out Point2 result)
    {
        result = tile;
        // ลองมุมตามไฟล์ก่อน แล้วค่อยลองแบบเอาพิกัดเป็นจุดกึ่งกลาง
        var candidates = new List<Point2>
        {
            tile,
            new Point2(tile.x - size.x / 2, tile.y - size.y / 2),
        };
        for (int r = 1; r <= 6; r++)
        {
            for (int dx = -r; dx <= r; dx++)
            {
                for (int dy = -r; dy <= r; dy++)
                {
                    if (Math.Abs(dx) == r || Math.Abs(dy) == r)
                    {
                        candidates.Add(new Point2(tile.x + dx, tile.y + dy));
                    }
                }
            }
        }
        foreach (Point2 c in candidates)
        {
            if (FitsAsPoi(c, size))
            {
                result = c;
                return true;
            }
        }
        return false;
    }

    private bool FitsAsPoi(Point2 at, Point2 size)
    {
        if (at.x < 0 || at.y < 0 || at.x + size.x > Terrain.Width || at.y + size.y > Terrain.Height)
        {
            return false;
        }
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // ⚠️ ใช้ oceans.dm เท่านั้น (IsLand/WaterDepthAt พัง — ดูหมายเหตุใน TouchesWater)
                if (Terrain.LandDistance(at.x + x, at.y + y) < 1)
                {
                    return false;
                }
                if (Terrain.BiomeAt(at.x + x, at.y + y) == Shared.Region.Biome.River)
                {
                    return false;
                }
            }
        }
        return !HasArtifactOverlapping(at, size);
    }

    /// <summary>ชุด POI รอบจุดเกิด (id ขึ้นต้น poi_near_) — วางที่ยังขาด</summary>
    private bool EnsureNearEntryPOIs()
    {
        bool placed = PlacePOISpots(spots: new[]
        {
            // ท่าเรือที่ 1 — ใกล้จุดเกิดที่สุดที่ยังติดแม่น้ำได้ (เกาะนี้แม่น้ำใกล้สุด ~47 tile)
            ("dock", (ushort)7001, new Point2(3, 3), 0, 5, true),
            // หลุมวาร์ปใกล้จุดเกิด
            ("camp_warphole", (ushort)9101, new Point2(6, 6), 0, 20, false),
        }, prefix: "poi_near_", nearEntry: true);
        if (placed)
        {
            Console.WriteLine("[world] วาง POI ชุดใกล้จุดเกิดแล้ว — ท่าเรือใกล้จุดเกิด + วาร์ปใกล้");
        }
        return placed;
    }

    /// <summary>วาง POI ตามรายการ spots ลงบนบก ไม่ทับของเดิม (nearEntry=true → วงค้นหาสูงสุด 60 tile จากจุดเกิด)
    /// WaterEdge=true → ต้องติดแม่น้ำ (มี tile River ล้อมรอบ footprint ≥2 จุด) — สำหรับท่าเรือ</summary>
    private bool PlacePOISpots((string Bp, ushort Type, Point2 Size, int MinInland, int MinDistFromEntry, bool WaterEdge)[] spots, string prefix, bool nearEntry)
    {
        Point2 entry = Terrain.EntryPoint;
        var rng = new Random();
        int placed = 0;
        // เกาะ ri35te: แม่น้ำใกล้จุดเกิด (40,177) เริ่มประมาณ 47 tile — วง 35 เดิมหา dock ไม่เจอ
        const int nearEntryMaxDist = 60;
        const int placeAttempts = 2000;

        // 🐛 เดิมเช็คแค่ "blueprint นี้มีอยู่แล้วไหม" ⇒ รายการที่ขอ **ชนิดเดียวกันสองอัน**
        //    (warp_accelerator ×2, camp_warphole ×2) ได้วางจริงแค่อันเดียว
        //    โลกใหม่เลยมีหลุมน้อยกว่าที่ตั้งใจ 2 จุด
        // ⇒ นับของที่มีอยู่แล้วแยกตามชนิด แล้วหักออกทีละอันตามรายการที่ขอ
        var have = new Dictionary<string, int>(StringComparer.Ordinal);
        var usedIds = new HashSet<string>(StringComparer.Ordinal);
        lock (_artifactLock)
        {
            foreach (string id in _artifacts.Keys)
            {
                usedIds.Add(id);
                if (!id.StartsWith(prefix, StringComparison.Ordinal))
                {
                    continue;
                }
                for (int s = 0; s < spots.Length; s++)
                {
                    string key = prefix + spots[s].Bp + "_";
                    if (id.StartsWith(key, StringComparison.Ordinal))
                    {
                        have[spots[s].Bp] = have.TryGetValue(spots[s].Bp, out int c) ? c + 1 : 1;
                        break;
                    }
                }
            }
        }

        int nextSuffix = 0;
        for (int i = 0; i < spots.Length; i++)
        {
            var (bp, type, size, minInland, minDist, waterEdge) = spots[i];
            // มีของชนิดนี้อยู่แล้ว 1 อัน = ตัดโควตาไป 1 อัน ที่เหลือยังต้องวาง
            if (have.TryGetValue(bp, out int already) && already > 0)
            {
                have[bp] = already - 1;
                continue;
            }
            bool found = false;
            for (int attempt = 0; attempt < placeAttempts; attempt++)
            {
                int tx, ty;
                if (nearEntry)
                {
                    // วงแหวน minDist..nearEntryMaxDist รอบจุดเกิด (วนหาจนได้ระยะขั้นต่ำ)
                    int d;
                    do
                    {
                        tx = entry.x + rng.Next(-nearEntryMaxDist, nearEntryMaxDist + 1);
                        ty = entry.y + rng.Next(-nearEntryMaxDist, nearEntryMaxDist + 1);
                        d = distSq(tx - entry.x, ty - entry.y);
                    }
                    while (d < minDist * minDist);
                    if (d > nearEntryMaxDist * nearEntryMaxDist)
                    {
                        continue;
                    }
                }
                else
                {
                    tx = rng.Next(4, Math.Max(5, Terrain.Width - 4 - size.x));
                    ty = rng.Next(4, Math.Max(5, Terrain.Height - 4 - size.y));
                }
                if (waterEdge)
                {
                    // [แก้เอง] เจ้าของสั่ง: ท่าเรือต้องอยู่ริมแม่น้ำเท่านั้น (ไม่ใช่ทะเล/ทะเลสาบทั่วไป)
                    // ⇒ เช็ค biome River ตรง ๆ แทน LandDistance (oceans.dm วัดระยะจากทะเลเท่านั้น ไม่รู้จักแม่น้ำ)
                    if (!TouchesRiver(tx, ty, size))
                    {
                        continue;
                    }
                }
                else if (Terrain.LandDistance(tx, ty) < minInland)
                {
                    continue;
                }
                bool allLand = true;
                for (int x = 0; x < size.x && allLand; x++)
                {
                    for (int y = 0; y < size.y && allLand; y++)
                    {
                        // [แก้เอง] ใช้ oceans.dm — IsLand/WaterDepthAt พัง (ดู TouchesWater)
                        if (Terrain.LandDistance(tx + x, ty + y) < 1)
                        {
                            allLand = false;
                        }
                        // [แก้เอง] oceans.dm วัดระยะจาก "ทะเล" เท่านั้น ไม่เห็นแม่น้ำ ⇒ กันพลาดสร้างทับแม่น้ำเอง
                        if (Terrain.BiomeAt(tx + x, ty + y) == Shared.Region.Biome.River)
                        {
                            allLand = false;
                        }
                    }
                }
                if (!allLand || HasArtifactOverlapping(new Point2(tx, ty), size))
                {
                    continue;
                }
                // [แก้เอง] ไม่วางกลางก้อนหิน/ป่าทึบ — ขอบรอบตัวมีของธรรมชาติได้ไม่เกิน 3 จุด
                // (เข้มกว่านี้หาที่วางไม่เจอเพราะป่าล้อมจุดเกิด)
                int ringNaturals = 0;
                for (int x = tx - 1; x <= tx + size.x; x++)
                {
                    if (Terrain.TryGetNatural(x, ty - 1, out _) && ++ringNaturals > 3) break;
                    if (Terrain.TryGetNatural(x, ty + size.y, out _) && ++ringNaturals > 3) break;
                }
                if (ringNaturals <= 3)
                {
                    for (int y = ty; y < ty + size.y && ringNaturals <= 3; y++)
                    {
                        if (Terrain.TryGetNatural(tx - 1, y, out _)) ringNaturals++;
                        if (Terrain.TryGetNatural(tx + size.x, y, out _)) ringNaturals++;
                    }
                }
                if (ringNaturals > 3)
                {
                    continue;
                }
                // เลขต่อท้ายต้องไม่ชนของที่มีอยู่ในเซฟ (วางเพิ่มทีหลังได้โดยไม่ทับของเดิม)
                string entityId;
                do
                {
                    entityId = prefix + bp + "_" + nextSuffix;
                    nextSuffix++;
                }
                while (usedIds.Contains(entityId));
                usedIds.Add(entityId);
                // [แก้เอง] เคลียร์ natural (ต้นไม้/หิน) ที่ทับ footprint — ไม่งั้นหลุมวาร์ปโดนหินบัง
                for (int x = 0; x < size.x; x++)
                {
                    for (int y = 0; y < size.y; y++)
                    {
                        Terrain.RemoveNatural(tx + x, ty + y);
                    }
                }
                AppearArtifact artifact = ArtifactFactory.Make(
                    null, entityId, type, new Point2(tx, ty), size,
                    Rotation.None, 0, 1, bp, BuildingState.Completed);
                AddArtifact(artifact, bp);
                Console.WriteLine("[world] วาง POI ธรรมชาติ {0} (tile {1},{2})", bp, tx, ty);
                placed++;
                found = true;
                break;
            }
            if (!found)
            {
                Console.WriteLine("[world] หาที่วาง {0} ไม่เจอ (พื้นที่ไม่พอ?) — ข้าม", bp);
            }
        }
        return placed > 0;

        static int distSq(int dx, int dy)
        {
            return dx * dx + dy * dy;
        }
    }

    /// <summary>ตรวจว่า footprint ที่ (tx,ty) ขนาด size ติดน้ำไหม
    /// ⚠️ ใช้ LandDistance (oceans.dm — พิสูจน์แล้ว) เท่านั้น ห้ามใช้ IsLand/WaterDepthAt
    /// (whole.ocean ตีความไม่ได้ ทำให้ IsLand คืนค่ามั่ว → เคยวางท่าเรือกลางบก)
    /// เกณฑ์: ตัวท่าทุก tile เป็นบก (≥1) + ขอบรอบตัวมีน้ำ/ฝั่ง (≤0) ≥2 จุด</summary>
    public bool TouchesWater(int tx, int ty, Point2 size)
    {
        int shore = 0;
        for (int x = tx - 1; x <= tx + size.x; x++)
        {
            if (Terrain.LandDistance(x, ty - 1) <= 0 && ++shore >= 2) return true;
            if (Terrain.LandDistance(x, ty + size.y) <= 0 && ++shore >= 2) return true;
        }
        for (int y = ty; y < ty + size.y; y++)
        {
            if (Terrain.LandDistance(tx - 1, y) <= 0 && ++shore >= 2) return true;
            if (Terrain.LandDistance(tx + size.x, y) <= 0 && ++shore >= 2) return true;
        }
        return false;
    }

    /// <summary>ตรวจว่า footprint ที่ (tx,ty) ขนาด size ติด "แม่น้ำ" โดยเฉพาะไหม (คนละอันกับ TouchesWater)
    /// [แก้เอง] เจ้าของสั่ง: ท่าเรือต้องอยู่ริมแม่น้ำเท่านั้น ไม่นับทะเล/ทะเลสาบ — oceans.dm (ที่ TouchesWater
    /// ใช้) วัดระยะจากทะเลเท่านั้น ไม่รู้จักแม่น้ำ ⇒ เช็ค biome River ตรง ๆ จาก whole.biomes แทน
    /// เกณฑ์เดียวกับ TouchesWater: ขอบรอบ footprint ต้องมี tile แม่น้ำอย่างน้อย 2 จุด</summary>
    public bool TouchesRiver(int tx, int ty, Point2 size)
    {
        int shore = 0;
        for (int x = tx - 1; x <= tx + size.x; x++)
        {
            if (Terrain.BiomeAt(x, ty - 1) == Shared.Region.Biome.River && ++shore >= 2) return true;
            if (Terrain.BiomeAt(x, ty + size.y) == Shared.Region.Biome.River && ++shore >= 2) return true;
        }
        for (int y = ty; y < ty + size.y; y++)
        {
            if (Terrain.BiomeAt(tx - 1, y) == Shared.Region.Biome.River && ++shore >= 2) return true;
            if (Terrain.BiomeAt(tx + size.x, y) == Shared.Region.Biome.River && ++shore >= 2) return true;
        }
        return false;
    }

    /// <summary>สิ่งปลูกสร้างทั้งหมด ณ ตอนนี้ — ใช้ส่งให้ผู้เล่นที่เพิ่งเข้ามา</summary>
    private double _nextRegrowCheckAt;

    /// <summary>
    /// [regrow] ต้นไม้/หิน/บ่อโคลนที่ผู้เล่นเก็บจนหมดหรือทำลาย งอกกลับหลัง World.NaturalRegrowSeconds
    /// (เกมต้นฉบับมี eco simulation ให้ทรัพยากรฟื้น — ของเราเดิมหายถาวร 108 ต้นแล้วบนเกาะเทส)
    /// ไม่งอกถ้ามีสิ่งปลูกสร้างทับ tile นั้นอยู่ · client ที่อยู่ใกล้ได้ AppearEntityOnTile · ที่เหลือเห็นตอนโหลด chunk ใหม่
    /// </summary>
    private void TickNaturalRegrowth(double now)
    {
        if (now < _nextRegrowCheckAt) { return; }
        _nextRegrowCheckAt = now + 30.0;
        double seconds = ServerConfig.Current.World.NaturalRegrowSeconds;
        if (seconds <= 0) { return; }
        List<(int x, int y)> due = Terrain.DueRegrow(now - seconds);
        int grown = 0;
        foreach ((int x, int y) in due)
        {
            if (HasArtifactOverlapping(new Point2(x, y), new Point2(1, 1)))
            {
                continue;                   // มีบ้านทับอยู่ — รอจนกว่าจะทุบ
            }
            ushort type = Terrain.RestoreNatural(x, y);
            if (type == 0) { continue; }
            grown++;
            MarkDirty();
            // [3 ก.ย. 2026] 🐛 **ต้นเหตุคนหลุดยกแผง** — เดิมส่ง AppearEntityOnTile ให้ client ที่อยู่ใกล้
            //   แต่ message นี้ **ไม่มี TypeCode ทั้งฝั่ง server และ client และ client ก็ไม่มี handler**
            //   ⇒ MessagePacking.Pack คืน false ("Not registered message") ⇒ SerializeMsg คืน 0 ไบต์
            //   ⇒ throw "Outbound packet exceeds buffer capacity: 0 bytes" ในลูปส่งของ BroadcastNear
            //   ⇒ ทุกคนที่อยู่ใกล้จุดที่ต้นไม้งอก **หลุดพร้อมกัน** (เจอ 1502 ครั้งใน log · มีมาตั้งแต่ 08:46)
            //   เอาการส่งออก — regrowth ยังทำงาน (RestoreNatural + MarkDirty) ผู้เล่นเห็นต้นไม้ที่งอกใหม่
            //   ตอนโหลด chunk รอบนั้นใหม่ (เดินออกไปแล้วกลับมา / รีเข้าเกม) ตามที่คอมเมนต์หัวเมธอดบอกไว้
        }
        if (grown > 0)
        {
            Console.WriteLine("[natural] งอกกลับ {0} ต้น (ถูกเก็บไปนานเกิน {1:0} นาที)", grown, seconds / 60);
        }
    }

    private double _nextDeathBoxSweepAt;

    /// <summary>
    /// [TodoList/07] กล่องของตกที่หมดอายุแล้ว (id = deathbox_&lt;unix หมดอายุ&gt;_xxx) ถูกเก็บจากฝั่งโลกทุก 30 วิ
    /// — ตัวผู้เล่นเก็บเองตอนออนไลน์อยู่แล้ว ตัวนี้กันกรณีเจ้าของออฟไลน์/เซิร์ฟล้มแล้วกล่องค้างในโลกตลอดกาล
    /// </summary>
    private void SweepDeathBoxes(double now)
    {
        if (now < _nextDeathBoxSweepAt) { return; }
        _nextDeathBoxSweepAt = now + 30.0;
        List<string> expired = null;
        foreach (AppearArtifact art in SnapshotArtifacts())
        {
            string id = art.EntityId;
            if (id == null || !id.StartsWith("deathbox_", StringComparison.Ordinal)) { continue; }
            int sep = id.IndexOf('_', 9);
            if (sep < 0 || !long.TryParse(id.Substring(9, sep - 9), out long expiresAt)) { continue; }
            if (now >= expiresAt) { (expired ??= new List<string>()).Add(id); }
        }
        if (expired == null) { return; }
        foreach (string id in expired)
        {
            int left = GetBoxItems(id).Length;
            RemoveArtifact(id);
            AnnounceGone(id);
            Console.WriteLine("[death] กล่องของตก {0} หมดอายุ (เจ้าของไม่อยู่) — เก็บทิ้ง ของที่เหลือ {1} ชิ้น", id, left);
        }
    }

    public AppearArtifact[] SnapshotArtifacts()
    {
        lock (_artifactLock)
        {
            AppearArtifact[] all = new AppearArtifact[_artifacts.Count];
            _artifacts.Values.CopyTo(all, 0);
            return all;
        }
    }

    /// <summary>blueprint id ของ artifact ทั้งหมด (เรียงตรงกับ SnapshotArtifacts)</summary>
    public string[] SnapshotArtifactBlueprints()
    {
        lock (_artifactLock)
        {
            string[] all = new string[_artifacts.Count];
            int i = 0;
            foreach (var key in _artifacts.Keys)
            {
                _artifactBlueprints.TryGetValue(key, out string bp);
                all[i++] = bp;
            }
            return all;
        }
    }

    /// <summary>
    /// ตำแหน่งกึ่งกลาง (พิกัดโลก หน่วย tile*200) ของ POI "รอยแยก" (blueprint warp_accelerator) ทุกจุดบนเกาะนี้
    ///
    /// ในเกมต้นฉบับ ไดโนเสาร์เกิดเฉพาะบริเวณใกล้ "รอยแยก" เท่านั้น — รอยแยกในเกมนี้คือ visual ของ POI
    /// blueprint `warp_accelerator` (RecipeData.cs: warp_accelerator → model crack_02) AnimalSpawner ใช้ตำแหน่งนี้
    /// เป็นจุดยึดโซนแทนโซนคงที่จากไฟล์ config
    ///
    /// ⚠️ ต้องเรียก**หลัง** Load() วาง POI เสร็จแล้ว (EnsureNaturalPOIs รันท้าย Load()) — Program.cs
    /// เรียก world.Load() ก่อน world.Animals.SpawnInitial() อยู่แล้ว ลำดับถูกต้องโดยไม่ต้องแก้อะไรเพิ่ม
    /// ตำแหน่งไม่คงที่ข้ามเกาะ/ข้ามรอบบูต (terrain สุ่มใหม่ทุกครั้ง) จึงต้องอ่านสด ห้าม cache ข้ามเซิร์ฟ
    /// </summary>
    public WorldPosition[] GetCrackPositions()
    {
        AppearArtifact[] all = SnapshotArtifacts();
        var list = new List<WorldPosition>();
        for (int i = 0; i < all.Length; i++)
        {
            AppearArtifact a = all[i];
            if (!TryGetArtifactBlueprint(a.EntityId, out string bp) || bp != "warp_accelerator")
            {
                continue;
            }
            int sx = a.Size.x <= 0 ? 1 : a.Size.x;
            int sy = a.Size.y <= 0 ? 1 : a.Size.y;
            list.Add(new WorldPosition((a.Tile.x + sx / 2f) * 200f, (a.Tile.y + sy / 2f) * 200f));
        }
        return list.ToArray();
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

    /// <summary>ดึงของทั้งหมดออกจากกล่อง (ใช้คืนเจ้าของก่อนทุบสิ่งปลูกสร้าง)</summary>
    public List<Item> TakeAllFromBox(string boxId)
    {
        lock (_boxLock)
        {
            if (!_boxes.TryGetValue(boxId ?? string.Empty, out List<Item> box))
            {
                return new List<Item>();
            }
            _boxes.Remove(boxId);
            MarkDirty();
            return box;
        }
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

    // ─────────────────── Artifact Materials ───────────────────

    /// <summary>
    /// จองวัสดุก่อสร้างทั้ง request โดยตรวจเพดานของทุก slot ใต้ lock เดียวกัน
    /// ใช้หลัง caller ตรวจ item/tag แล้ว เพื่อกัน architect สองคนเติม slot เดียวกันเกินจำนวนพร้อมกัน.
    /// </summary>
    public bool TryReserveArtifactMaterials(string entityId, Dictionary<string, List<Item>> additions,
        Dictionary<string, int> slotMaximums)
    {
        if (string.IsNullOrEmpty(entityId) || additions == null || additions.Count == 0 || slotMaximums == null)
        {
            return false;
        }
        lock (_artifactMaterialsLock)
        {
            if (!_artifactMaterials.TryGetValue(entityId, out Dictionary<string, List<Item>> slots))
            {
                slots = new Dictionary<string, List<Item>>();
            }
            foreach (var pair in additions)
            {
                if (!slotMaximums.TryGetValue(pair.Key, out int maximum) || pair.Value == null)
                {
                    return false;
                }
                int existing = slots.TryGetValue(pair.Key, out List<Item> reserved) ? reserved.Count : 0;
                if (existing + pair.Value.Count > maximum)
                {
                    return false;
                }
            }
            if (!_artifactMaterials.ContainsKey(entityId))
            {
                _artifactMaterials[entityId] = slots;
            }
            foreach (var pair in additions)
            {
                if (!slots.TryGetValue(pair.Key, out List<Item> reserved))
                {
                    reserved = new List<Item>();
                    slots[pair.Key] = reserved;
                }
                reserved.AddRange(pair.Value);
            }
            _dirty = true;
            return true;
        }
    }

    /// <summary>ฝากวัสดุเข้า slot ของสิ่งปลูกสร้าง — คืน false ถ้า slot ไม่พบใน blueprint</summary>
    public bool PutArtifactMaterials(string entityId, string slotId, List<Item> items)
    {
        lock (_artifactMaterialsLock)
        {
            if (!_artifactMaterials.TryGetValue(entityId, out Dictionary<string, List<Item>> slots))
            {
                slots = new Dictionary<string, List<Item>>();
                _artifactMaterials[entityId] = slots;
            }
            if (!slots.TryGetValue(slotId, out List<Item> existing))
            {
                existing = new List<Item>();
                slots[slotId] = existing;
            }
            existing.AddRange(items);
            _dirty = true;
            return true;
        }
    }

    /// <summary>ดึงวัสดุทั้งหมดของสิ่งปลูกสร้าง (read-only snapshot) คืน null ถ้าไม่มี</summary>
    public Dictionary<string, List<Item>> GetArtifactMaterials(string entityId)
    {
        lock (_artifactMaterialsLock)
        {
            if (!_artifactMaterials.TryGetValue(entityId, out Dictionary<string, List<Item>> slots))
            {
                return null;
            }
            var result = new Dictionary<string, List<Item>>();
            foreach (var kv in slots)
            {
                result[kv.Key] = new List<Item>(kv.Value);
            }
            return result;
        }
    }

    /// <summary>ตรวจว่าสิ่งปลูกสร้างมีวัสดุฝากไว้หรือไม่ (ใช้ตอน build gate)</summary>
    public bool HasArtifactMaterials(string entityId)
    {
        lock (_artifactMaterialsLock)
        {
            return _artifactMaterials.TryGetValue(entityId ?? string.Empty, out var slots) && slots.Count > 0;
        }
    }

    /// <summary>ดึงวัสดุทั้งหมดของสิ่งปลูกสร้างออก (ใช้ตอนทุบ/ยกเลิก) คืน null ถ้าไม่มี</summary>
    public Dictionary<string, List<Item>> TakeArtifactMaterials(string entityId)
    {
        lock (_artifactMaterialsLock)
        {
            if (_artifactMaterials.TryGetValue(entityId ?? string.Empty, out Dictionary<string, List<Item>> slots))
            {
                _artifactMaterials.Remove(entityId);
                _dirty = true;
                return slots;
            }
            return null;
        }
    }

    /// <summary>Snapshot สำหรับเซฟ — เรียกภายใต้ lock เท่านั้น (ผ่าน SnapshotArtifactMaterials)</summary>
    private Dictionary<string, Dictionary<string, List<Item>>> SnapshotArtifactMaterialsInternal()
    {
        var result = new Dictionary<string, Dictionary<string, List<Item>>>();
        foreach (var kv in _artifactMaterials)
        {
            var slots = new Dictionary<string, List<Item>>();
            foreach (var slot in kv.Value)
            {
                slots[slot.Key] = new List<Item>(slot.Value);
            }
            result[kv.Key] = slots;
        }
        return result;
    }

    /// <summary>Snapshot วัสดุที่ฝากไว้ทุกชิ้น — เรียกตอน save</summary>
    public Dictionary<string, Dictionary<string, List<Item>>> SnapshotArtifactMaterials()
    {
        lock (_artifactMaterialsLock)
        {
            return SnapshotArtifactMaterialsInternal();
        }
    }

    /// <summary>Restore วัสดุจากเซฟ — เรียกตอน load</summary>
    public void RestoreArtifactMaterials(string entityId, Dictionary<string, List<Item>> slots)
    {
        lock (_artifactMaterialsLock)
        {
            _artifactMaterials[entityId] = slots;
        }
    }

    // ─────────────────── Generator ───────────────────

    /// <summary>
    /// GP-03: ขอ generator ของจุดนี้ ยังไม่เคยมีก็สร้างใหม่จาก factory
    /// คืน "สำเนา" เสมอ เพื่อไม่ให้ผู้เรียกไปแก้ของกลางโดยไม่ผ่าน lock
    /// </summary>
    /// <summary>ล้าง cache จุดธรรมชาติ เพื่อให้แตะรอบหน้าสร้าง generator จาก gathering_tools.json ใหม่</summary>
    public int ForgetNaturalGeneratorCache()
    {
        lock (_genLock)
        {
            List<string> keys = new List<string>();
            foreach (string key in _generators.Keys)
            {
                if (key.StartsWith("natural_", StringComparison.Ordinal))
                {
                    keys.Add(key);
                }
            }
            for (int i = 0; i < keys.Count; i++)
            {
                _generators.Remove(keys[i]);
            }
            return keys.Count;
        }
    }

    public Generator[] GetOrCreateGenerators(string naturalId, ushort entityType, Func<ushort, List<Generator>> factory)
    {
        lock (_genLock)
        {
            if (!_generators.TryGetValue(naturalId, out List<Generator> gens))
            {
                gens = factory(entityType);
                _generators[naturalId] = gens;
            }
            int want = ServerConfig.ResourceLevel;
            for (int i = 0; i < gens.Count; i++)
            {
                Generator g = gens[i];
                if (g.Level != want)
                {
                    g.Level = want;
                    gens[i] = g;
                }
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
    /// <param name="ranOut">true ถ้า**ทุก**ชนิดของจุดนี้หมดแล้ว (เช่น ทั้งกิ่งไม้และท่อนไม้) — ไม่ใช่แค่ชนิดที่เพิ่งจอง</param>
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
            // 🐛 เดิมพอ generator ชนิดที่จองถึงหน่วยสุดท้าย (เช่น "กิ่งไม้") จะ Remove(naturalId)
            // ทั้งก้อนทันที ⇒ ต้นไม้ที่มีทั้งกิ่งไม้+ท่อนไม้ (ดู NaturalData.cs) หายไปทั้งต้นทั้งที่
            // ท่อนไม้ยังไม่ได้เก็บเลย ผู้เล่นรายงาน "เก็บแค่กิ่งไม้ แต่ต้นไม้หายไปก่อน" ตรงนี้เป๊ะ
            // แก้ให้เหมือน TryReserveCorpsePart ด้านล่าง: เอาออกแค่ชนิดที่หมด (RemoveAt) แล้วค่อยเช็คว่า
            // "ทุกชนิด" ในจุดนี้หมดหรือยัง (gens.Count == 0) ถึงจะถือว่า naturalId นี้หมดจริง
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
                _generators.Remove(naturalId);
                ranOut = true;
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
            try
            {
                snapshot[i].Process();
            }
            catch (Exception e)
            {
                Console.WriteLine($"[world] player {snapshot[i].EntityId} process failed: {e.Message}");
            }
        }
        try { Animals.Process(); }
        catch (Exception e) { Console.WriteLine($"[world] animals process failed: {e.Message}"); }
        double now = Durango.Utils.Times.UnixTimeNow();
        try { Weather.Process(now, snapshot); }
        catch (Exception e) { Console.WriteLine($"[world] weather process failed: {e.Message}"); }
        try { SweepDeathBoxes(now); }
        catch (Exception e) { Console.WriteLine($"[world] death box sweep failed: {e.Message}"); }
        if (ServerConfig.Current.Features.WarpAccelerator)
        {
            try { WarpAccelerators.Process(now); }
            catch (Exception e) { Console.WriteLine($"[world] warp process failed: {e.Message}"); }
        }
        try { TickNaturalRegrowth(now); }
        catch (Exception e) { Console.WriteLine($"[world] natural regrowth failed: {e.Message}"); }
        try { TickFarms(now); }
        catch (Exception e) { Console.WriteLine($"[world] farms process failed: {e.Message}"); }
        // ใครเข้า/ออกระยะมองเห็นบ้าง — ต้องทำหลัง Process เพราะตำแหน่งเพิ่งขยับ
        try { TickVisibility(now); }
        catch (Exception e) { Console.WriteLine($"[world] visibility process failed: {e.Message}"); }
    }

    // ── Party management ──────────────────────────────────────────────

    private readonly Dictionary<string, List<ServerPlayer>> _parties = new Dictionary<string, List<ServerPlayer>>();
    private readonly object _partyLock = new object();

    public void CreateParty(string partyId, ServerPlayer leader)
    {
        lock (_partyLock)
        {
            _parties[partyId] = new List<ServerPlayer> { leader };
        }
    }

    /// <summary>
    /// เพิ่มสมาชิกเข้า party แบบ atomic — ตรวจ capacity และ add ภายใต้ lock เดียว
    /// กัน race ที่ invite สองคำสั่งพร้อมกันแล้ว party เกินโควตา
    /// </summary>
    public bool TryAddToParty(string partyId, ServerPlayer player, int maxMembers)
    {
        lock (_partyLock)
        {
            if (!_parties.TryGetValue(partyId, out List<ServerPlayer> members))
            {
                members = new List<ServerPlayer>();
                _parties[partyId] = members;
            }
            if (members.Contains(player))
            {
                return true;
            }
            if (members.Count >= maxMembers)
            {
                return false;
            }
            members.Add(player);
            return true;
        }
    }

    public void RemoveFromParty(ServerPlayer player)
    {
        if (player.PartyId == null) return;
        lock (_partyLock)
        {
            if (_parties.TryGetValue(player.PartyId, out List<ServerPlayer> members))
            {
                members.Remove(player);
                if (members.Count == 0)
                {
                    _parties.Remove(player.PartyId);
                }
            }
        }
    }

    public List<ServerPlayer> GetPartyMembers(string partyId)
    {
        if (partyId == null) return new List<ServerPlayer>();
        lock (_partyLock)
        {
            if (_parties.TryGetValue(partyId, out List<ServerPlayer> members))
            {
                return new List<ServerPlayer>(members);
            }
        }
        return new List<ServerPlayer>();
    }

    public int GetPartyMemberCount(string partyId)
    {
        if (partyId == null) return 0;
        lock (_partyLock)
        {
            if (_parties.TryGetValue(partyId, out List<ServerPlayer> members))
            {
                return members.Count;
            }
        }
        return 0;
    }

    public ServerPlayer GetPartyLeader(string partyId)
    {
        var members = GetPartyMembers(partyId);
        for (int i = 0; i < members.Count; i++)
        {
            if (members[i].IsPartyLeader)
            {
                return members[i];
            }
        }
        return null;
    }

    // ── Clan registry ─────────────────────────────────────────────────

    private List<ClanSave> _clans = new List<ClanSave>();
    private bool _clansDirty;

    public void LoadClans(WorldSave worldSave)
    {
        if (worldSave.Clans != null)
        {
            _clans = worldSave.Clans;
        }
    }

    public void FillClans(WorldSave worldSave)
    {
        worldSave.Clans = _clans;
    }

    public void AddClan(ClanSave clan)
    {
        lock (_lock)
        {
            _clans.Add(clan);
            _clansDirty = true;
        }
        MarkDirty();
    }

    public ClanSave GetClan(string clanId)
    {
        lock (_lock)
        {
            for (int i = 0; i < _clans.Count; i++)
            {
                if (_clans[i].Id == clanId) return _clans[i];
            }
        }
        return null;
    }

    public void RemoveFromClan(string clanId, string entityId)
    {
        lock (_lock)
        {
            ClanSave clan = GetClan(clanId);
            if (clan == null) return;
            clan.MemberEntityIds.Remove(entityId);
            if (clan.MemberEntityIds.Count == 0)
            {
                _clans.Remove(clan);
            }
            _clansDirty = true;
        }
        MarkDirty();
    }

    public bool SaveClansIfDirty()
    {
        if (!_clansDirty) return false;
        _clansDirty = false;
        return true;
    }
}
