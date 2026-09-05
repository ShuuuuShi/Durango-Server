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
using DurangoServer.Modding;
using Messages;
using DurangoServer.Modding;
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

// ServerPlayer.Gathering — ดูรายละเอียดที่ docs/server/ServerPlayer.Gathering.md

public partial class ServerPlayer
{

    /// <summary>
    /// GP-09: ระยะที่มือเอื้อมถึง (หน่วย tile) — client จำกัดไว้ใกล้กว่านี้อยู่แล้ว
    /// เผื่อไว้กว้างหน่อยเพราะตำแหน่งฝั่ง server คือ "ปลายทางของ Move ล่าสุด" ไม่ใช่ตำแหน่งระหว่างเดิน
    /// </summary>
    private const int MaxReachTiles = 8;

    /// <summary>GP-09: ผู้เล่นอยู่ใกล้ tile นี้พอจะแตะถึงไหม (1 tile = 200 หน่วยโลก)</summary>
    private bool IsWithinReach(Point2 tile)
    {
        return IsWithinReach(tile, MaxReachTiles);
    }

    /// <summary>เหมือนข้างบนแต่กำหนดระยะเอง — โต๊ะคราฟต์ใช้ระยะแคบกว่าเพราะต้อง "ยืนที่โต๊ะ" จริง ๆ</summary>
    private bool IsWithinReach(Point2 tile, float rangeTiles)
    {
        WorldPosition pos = CurrentPosition;
        float dx = pos.x / 200f - tile.x;
        float dy = pos.y / 200f - tile.y;
        return dx * dx + dy * dy <= rangeTiles * rangeTiles;
    }

    /// <summary>รหัสเมนู interaction ที่ client รู้จัก (client/InteractionData/Interaction.cs)</summary>
    private const int InteractionAttack = 1;        // "공격!" — ปุ่มโจมตี
    private const int InteractionCollect = 506;
    private const int InteractionRemoveNatural = 10268;
    private const int InteractionBuildArtifact = 101;
    private const int InteractionWarp = 515;         // "워프" — เปิดแผนที่โหมดวาป (WorldMapGroup.OpenForWarp)

    private void HandleTouch(Touch msg, PacketHeader header)
    {
        if (msg.EntityType <= 0)
        {
            return;
        }
        if (Dead)                       // เฟส C รอบ 2: ตายแล้วแตะอะไรไม่ได้
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // สัตว์: entity type 2000-2999 และ client ส่ง Tile = (-1,-1) มาเสมอ
        // (client/InteractionObject.cs → Tile คืน -Vector2.one ถ้าเป้าเป็น Animal)
        //
        // 🐛 เดิมไม่มีเคสนี้ ทุกอย่างเลยตกไปทางของธรรมชาติ → ตอบ Touched ที่ Interactions ว่าง
        // = แตะสัตว์แล้ว **ปุ่มโจมตีไม่ขึ้น** เพราะเมนูฝั่ง client มาจาก Touched.Interactions ล้วน ๆ
        if (_world.Animals.TryGet(msg.EntityId, out ServerAnimal animal))
        {
            HandleTouchAnimal(animal, header);
            return;
        }
        // H-5: id ต้องมาจาก "พิกัด" เท่านั้น ห้ามใช้ค่าที่ client ส่งมา
        //
        // เดิมใช้ msg.EntityId เป็นคีย์ของ state จำนวนที่เหลือ ⇒ ยืนที่เดิมแล้วเปลี่ยน id ไปเรื่อย ๆ
        // ("a1", "a2", "a3", ...) จะได้ generator ชุดใหม่เต็มจำนวนทุกครั้ง = **เก็บของจากต้นเดียวไม่มีวันหมด**
        // แถม _generators / _naturalTiles ก็โตไม่จำกัดตามจำนวน id ที่ client คิดขึ้นมา (memory leak ที่ยิงได้จากนอก)
        string naturalId = $"natural_{msg.Tile.x}_{msg.Tile.y}";
        Touched reply = new Touched
        {
            EntityId = naturalId,
            Level = ServerConfig.ResourceLevel
        };
        if (msg.EntityType >= 10000)
        {
            // GP-09: ของธรรมชาติต้องมีอยู่จริงที่ tile นั้นตาม garden ของ server
            // เดิมสร้าง generator ให้ทุก tile ที่ client ขอมา = ขุดอากาศได้ไม่จำกัด
            if (!_world.Terrain.TryGetNatural(msg.Tile.x, msg.Tile.y, out ushort actualType))
            {
                Console.WriteLine("[touch] ปฏิเสธ {0}: ไม่มีของธรรมชาติที่ tile {1},{2}", Name, msg.Tile.x, msg.Tile.y);
                Send(Aborts.Reason(), header.Seq);
                return;
            }
            if (!IsWithinReach(msg.Tile))
            {
                Console.WriteLine("[touch] ปฏิเสธ {0}: tile {1},{2} อยู่ไกลเกินเอื้อม", Name, msg.Tile.x, msg.Tile.y);
                Send(Aborts.Reason(), header.Seq);
                return;
            }
            if (actualType != msg.EntityType)
            {
                // ชนิดของที่ได้ต้องมาจาก garden ไม่ใช่จาก client ไม่งั้นเลือกได้ว่าจะให้ต้นไม้ออกอะไร
                Console.WriteLine("[touch] {0}: client อ้างชนิด {1} แต่ tile {2},{3} เป็น {4} — ใช้ของ server",
                    Name, msg.EntityType, msg.Tile.x, msg.Tile.y, actualType);
            }
            // ผูก id กับ tile ไว้ให้ Collect ใช้ (Collect จะไม่อ่าน Tile ที่ client ส่งมาอีก)
            _world.RegisterNaturalTile(naturalId, msg.Tile);
            reply.Interactions = new[] { InteractionCollect, InteractionRemoveNatural };
            // GP-03: state อยู่ที่ world แล้ว ทุกคนเห็นจำนวนที่เหลือชุดเดียวกัน
            reply.Collectible = new Collectible
            {
                EntityId = naturalId,
                CollectibleId = null,
                Size = null,
                Generators = _world.GetOrCreateGenerators(naturalId, actualType, MakeGenerators),
                CriticalGenerator = null
            };
        }
        else if (RecipeData.BlueprintByType.TryGetValue(msg.EntityType, out string blueprintId))
        {
            // ต้องเป็น id ของ artifact จริง — เดิมค้างเป็น natural_x_y จากบรรทัดบน
            // ทำให้บางเส้นทางทุบส่ง id ผิด แล้วเซิร์ฟตอบ "ไม่รู้จัก entity"
            reply.EntityId = msg.EntityId;
            var interactions = new List<int> { 103 };
            // A placed blueprint is still Occupied. Expose BuildArtifact so the
            // client opens the material ledger only when the player clicks it.
            if (_world.TryGetArtifact(msg.EntityId, out AppearArtifact touchedArtifact)
                && touchedArtifact.States.BuildingState == BuildingState.Occupied
                && IsWithinReach(touchedArtifact.Tile))
            {
                interactions.Insert(0, InteractionBuildArtifact);
            }
            // แปลงผัก — เมนู ปลูก/ใส่ปุ๋ย/รดน้ำ/ถอน มาจาก component "Growable"
            // และถ้าโตแล้วให้เมนู "เก็บ" ชุดเดียวกับของธรรมชาติ (client ไม่มีเมนูเก็บเกี่ยวแยก)
            if (ServerConfig.Current.Features.Farming && ServerWorld.IsFarmBlueprint(blueprintId))
            {
                AddFarmInteractions(msg.EntityId, interactions, ref reply);
            }
            if (RecipeData.BlueprintName.TryGetValue(blueprintId, out string bpName))
            {
                reply.EntityName = bpName;
            }
            // [4 ก.ย. 2026] เมนูฟังก์ชันของสิ่งปลูกสร้าง (คราฟต์/พัก/เปิดกล่อง/ชุบชีวิต) ต้องโผล่
            // **เฉพาะเมื่อสร้างเสร็จแล้ว** — เดิมเพิ่มทุก component ทันทีที่แตะ ⇒ กล่องที่ยังเป็นไซต์
            // (ยังไม่ใส่ของ) ก็เปิด UI เก็บของได้ · กองไฟไซต์ก็โชว์เมนูพัก/ชุบชีวิต (เจ้าของรายงาน)
            // Warphole มีการ์ด built อยู่แล้วในเคสของมันเอง
            bool artifactBuilt = _world.TryGetArtifact(msg.EntityId, out AppearArtifact compArtifact)
                && (compArtifact.States.BuildingState == BuildingState.Built
                    || compArtifact.States.BuildingState == BuildingState.Completed);
            if (RecipeData.BlueprintComponents.TryGetValue(blueprintId, out string[] comps))
            {
                for (int i = 0; i < comps.Length; i++)
                {
                    switch (comps[i])
                    {
                        case "Workbench":
                            if (artifactBuilt) { interactions.Add(501); }
                            break;
                        case "Shelter":
                            if (artifactBuilt) { interactions.Add(407); }
                            break;
                        case "Sanctum":
                            // 503 = Resurrect ("ปั้มหัวใจ" ชุบชีวิตที่ศาลเจ้า/กองไฟ) — โผล่เฉพาะตอนตายจริง
                            // (เดิมโชว์ตลอดแม้ยังไม่ตาย ⇒ กองไฟมีเมนูปั้มหัวใจติดมาด้วย — เจ้าของรายงาน)
                            if (artifactBuilt && Dead) { interactions.Add(503); }
                            break;
                        case "Bandstand":
                            // 552 = HostConcert — คอนเสิร์ต/วงดนตรีเป็นฟีเจอร์ที่ตัดออกจากรอบนี้ ไม่ต้องโชว์
                            break;
                        case "WarpAccelerator":
                            // เมนูของ warp_accelerator ขึ้นกับสถานะกิจกรรมสด ๆ (ว่าง/กำลังไป/รอรับรางวัล)
                            // ไม่ใช่ค่าคงที่ตัวเดียวเหมือน component อื่น ๆ ข้างบน — ดู ServerPlayer.WarpAccelerator.cs
                            AddWarpAcceleratorInteractions(msg.EntityId, interactions);
                            break;
                        case "Inventory":
                            // 404 = เปิดกล่องเก็บของ — ต้องสร้างเสร็จก่อน (เดิมเปิดได้ตั้งแต่ยังเป็นไซต์)
                            if (artifactBuilt) { interactions.Add(404); }
                            break;
                        case "Warphole":
                            // [แก้เอง] 23 ส.ค. 2026 — เจ้าของรายงาน "ไม่มีเมนูกดวาป" ที่หลุมวาร์ป
                            // ต้นเหตุ: component นี้ (camp_warphole/neutral_warphole ทั้งคู่มี tag "Warphole"
                            // ติดมาด้วยเสมอ — ดู RecipeData.BlueprintComponents) ไม่เคยถูกจับใน switch นี้เลย
                            // เมนู "วาป" (Interaction.Warp=515) เลยไม่โผล่ตอนแตะหลุมวาร์ปสักครั้ง — เพิ่มเข้าไป
                            // client กดแล้วจะส่ง IsWarpholeAvailable ตามมา (ดู HandleIsWarpholeAvailable ใน
                            // ServerPlayer.POI.cs) ก่อนเปิดแผนที่โหมดวาปจริง
                            // camp_warphole is the only warp implementation enabled
                            // in this pass. neutral/cargo shares this component but
                            // must not expose a usable Warp interaction.
                            if (ServerConfig.Current.Features.IslandTravel
                                && BlueprintPOIType.TryGetValue(blueprintId, out var warpType)
                                && warpType == Shared.System.PointOfInterest.Warphole
                                && _world.TryGetArtifact(msg.EntityId, out AppearArtifact warpArtifact)
                                && (warpArtifact.States.BuildingState == BuildingState.Built
                                    || warpArtifact.States.BuildingState == BuildingState.Completed))
                            {
                                interactions.Add(InteractionWarp);
                            }
                            break;
                    }
                }
            }
            reply.Interactions = interactions.ToArray();
            Console.WriteLine("[touch] artifact {0} type={1} blueprint={2} interactions={3}", msg.EntityId, msg.EntityType, blueprintId, interactions.Count);
        }
        Console.WriteLine("[touch] {0} type={1} tile={2},{3} gens={4}", naturalId, msg.EntityType, msg.Tile.x, msg.Tile.y, reply.Collectible.Generators?.Length ?? 0);
        Send(reply, header.Seq);
    }

    /// <summary>ระยะที่แล่ซากได้ (เท่าระยะเอื้อมของ tile แต่คิดจากตำแหน่งซากตรง ๆ)</summary>
    private const float ButcheryRange = MaxReachTiles * 200f;

    /// <summary>
    /// แตะสัตว์ — ยังไม่ตายให้ปุ่ม "โจมตี" · ตายแล้วให้เมนูแล่เนื้อ
    ///
    /// ซากถูกย้ายไป layer ของ prop ฝั่ง client (AnimalBehavior.OnDie) จึงแตะได้เหมือนของธรรมชาติ
    /// และเมนูแล่เนื้อคือ Collectible ชุดเดียวกับการเก็บของ — client ไม่มีเมนู "แล่" แยกต่างหาก
    /// </summary>
    private void HandleTouchAnimal(ServerAnimal animal, PacketHeader header)
    {
        SpawnTable.Entry entry = SpawnTable.Find(animal.EntityType);
        Touched reply = new Touched
        {
            EntityId = animal.EntityId,
            EntityName = entry?.Name ?? (AnimalData.TryGet(animal.EntityType, out AnimalData.AnimalInfo info) ? info.Name : null),
            Level = animal.Level
        };

        if (animal.IsAlive)
        {
            reply.Interactions = new[] { InteractionAttack };
            Console.WriteLine("[touch] {0} แตะ {1} ({2} lv{3}) — ให้ปุ่มโจมตี", Name, animal.EntityId, reply.EntityName, animal.Level);
            Send(reply, header.Seq);
            return;
        }

        // ซาก: generator ถูกสร้างไว้ตั้งแต่ตอนตาย (AnimalSpawner.Damage) เพื่อให้ทุกคนเห็นชุดเดียวกัน
        Generator[] parts = _world.PeekGenerators(animal.EntityId);
        if (parts == null || parts.Length == 0)
        {
            Console.WriteLine("[touch] {0} แตะซาก {1} แต่แล่หมดแล้ว", Name, animal.EntityId);
            Send(reply, header.Seq);
            return;
        }
        WorldPosition me = CurrentPosition;
        float dx = animal.Position.x - me.x;
        float dy = animal.Position.y - me.y;
        if (dx * dx + dy * dy > ButcheryRange * ButcheryRange)
        {
            Console.WriteLine("[touch] ปฏิเสธ {0}: ซาก {1} อยู่ไกลเกินเอื้อม", Name, animal.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        reply.Collectible = new Collectible
        {
            EntityId = animal.EntityId,
            CollectibleId = null,
            Size = null,
            Generators = parts,
            CriticalGenerator = null
        };
        Console.WriteLine("[touch] {0} แตะซาก {1} — แล่ได้ {2} ชิ้นส่วน", Name, animal.EntityId, parts.Length);
        Send(reply, header.Seq);
    }

    /// <summary>
    /// ของแต่ละอย่างต้องใช้เครื่องมืออะไร — อ่านจาก <c>data/gathering_tools.json</c>
    /// ไม่มีในไฟล์ = มือเปล่า (ผลไม้ · ใบไม้ · ลำต้น · ปลา · ดินเหนียว)
    /// เครื่องมือเป็น tag ของไอเทม (axe / knife / pickaxe) ไม่ใช่ชื่อไอเทม
    /// </summary>
    private static Dictionary<string, int> ToolRequirementFor(string prototype)
    {
        string tag = GatheringTools.RequiredTag(prototype);
        if (!string.IsNullOrEmpty(tag) && tag != "bare_hands")
        {
            return new Dictionary<string, int> { { tag, 1 } };
        }
        return new Dictionary<string, int> { { "bare_hands", 1 } };
    }

    /// <summary>
    /// GP-08b: ผู้เล่นถือเครื่องมือที่ generator นี้ต้องการอยู่จริงไหม
    ///
    /// **ไม่เชื่อ `ToolItemId` ที่ client ส่งมาเฉย ๆ** — ต้องเป็นไอเทมที่อยู่ในกระเป๋าจริง
    /// และมี tag ตรงกับที่ขอ ระดับไม่ต่ำกว่าที่กำหนด
    /// ถ้า client ไม่ได้ส่ง id มา ก็ค้นในกระเป๋าให้เองว่ามีอะไรใช้ได้ไหม (บอทไม่ต้องรู้เรื่อง tag)
    /// </summary>
    private bool HasRequiredTool(Generator gen, string toolItemId, out string missingTag, out Item tool)
    {
        missingTag = null;
        tool = default;
        Dictionary<string, int> need = gen.ToolRequirements;
        if (need == null || need.Count == 0 || need.ContainsKey("bare_hands"))
        {
            return true;
        }
        lock (_inventory)
        {
            foreach (KeyValuePair<string, int> req in need)
            {
                for (int i = 0; i < _inventory.Count; i++)
                {
                    Item it = _inventory[i];
                    if (!string.IsNullOrEmpty(toolItemId) && it.Id != toolItemId)
                    {
                        continue;       // client ระบุชิ้นไหนมา ก็ตรวจชิ้นนั้น
                    }
                    if (ItemTagData.LevelOf(it.Prototype, req.Key) >= req.Value)
                    {
                        tool = it;
                        return true;
                    }
                }
                missingTag = req.Key;
            }
        }
        return false;
    }

    /// <summary>
    /// ตรวจเครื่องมือก่อนจอง generator — ไม่ผ่านจะตอบ `ToolNeeded` ที่ client เอาไปขึ้นหน้าต่าง
    /// "ต้องใช้เครื่องมือ" พร้อมรายการสูตรที่คราฟต์เครื่องมือนั้นได้
    /// </summary>
    /// <param name="usedToolId">
    /// id ของเครื่องมือที่ใช้จริง (null = มือเปล่า) — เอาไปหักความทนทานตอนทำสำเร็จ
    /// **ต้องหักตอนสำเร็จ ไม่ใช่ตอนนี้** เพราะการจองอาจล้มเหลว (คนอื่นชิงไปก่อน)
    /// แล้วเครื่องมือจะสึกฟรีทั้งที่ไม่ได้ของ
    /// </param>
    private bool CheckToolBeforeCollect(string entityId, string generatorId, string toolItemId, PacketHeader header,
        out string usedToolId)
    {
        usedToolId = null;
        Generator[] gens = _world.PeekGenerators(entityId);
        if (gens == null)
        {
            return true;            // ไม่มี state = ปล่อยให้ขั้นตอนจองปฏิเสธเอง
        }
        for (int i = 0; i < gens.Length; i++)
        {
            if (gens[i].Id != generatorId)
            {
                continue;
            }
            if (HasRequiredTool(gens[i], toolItemId, out string missing, out Item used))
            {
                usedToolId = used.Id;
                return true;
            }
            Console.WriteLine("[collect] ปฏิเสธ {0}: {1} ต้องใช้{2}", Name, gens[i].Name, ToolNameOf(missing));
            SendToolNeeded(missing, header.Seq);
            return false;
        }
        return true;
    }

    /// <summary>
    /// เครื่องมือสึก 1 ครั้งใช้งาน — หมดหลอดแล้วพังหายไปจากกระเป๋า
    ///
    /// เรียกตอน **ทำสำเร็จแล้วเท่านั้น** (ในบล็อก deferred) ไม่ใช่ตอนกดเก็บ
    /// เพราะระหว่างรอเวลาเก็บ อาจถูกคนอื่นชิงหน่วยไปก่อนแล้วเราไม่ได้อะไรเลย
    ///
    /// ของที่ไม่มีหลอด (MaxOf = 0) และตอนปิดระบบใน config จะไม่โดนอะไรทั้งนั้น
    /// </summary>
    private void WearTool(string toolItemId, WearKind kind = WearKind.Collect)
    {
        if (!ServerConfig.Current.Features.ToolDurability) return;
        if (string.IsNullOrEmpty(toolItemId))
        {
            return;                 // มือเปล่า
        }
        ToolConfig cfg = ServerConfig.Current.Tools;
        // [TodoList/03] หักตามชนิดงาน (คราฟต์ 3.2 / เก็บ 1.6 …) — Deltas ปิด = WearPerUse เดิม
        bool deltas = cfg.Deltas != null && cfg.Deltas.Enabled;
        if (!cfg.Enabled || (!deltas && cfg.WearPerUse <= 0f))
        {
            return;
        }
        string brokenName = null;
        float left = 0f, max = 0f, wear = 0f;
        lock (_inventory)
        {
            for (int i = 0; i < _inventory.Count; i++)
            {
                Item it = _inventory[i];
                if (it.Id != toolItemId)
                {
                    continue;
                }
                if (!ToolDurability.HasDurability(it))
                {
                    return;         // ไม่ใช่เครื่องมือที่สึกได้
                }
                max = ToolDurability.MaxOf(it);
                wear = ToolDurability.WearFor(kind, it.Prototype);
                left = ToolDurability.RemainingOf(it) - wear;
                if (left <= 0f)
                {
                    brokenName = it.Name;
                    _inventory.RemoveAt(i);
                    UnequipItemEverywhere(it.Id);
                    ForgetInventoryItem(it.Id);
                }
                else
                {
                    it.Durability = ToolDurability.MakeGauge(left, max);
                    _inventory[i] = it;      // Item เป็น struct — ต้องเขียนกลับเข้า list
                }
                break;
            }
        }
        if (max <= 0f)
        {
            return;                 // ไม่เจอชิ้นนั้น (ทิ้งไปแล้ว/ย้ายเข้ากล่อง)
        }
        MarkDirty();
        SendInventory();
        if (brokenName != null)
        {
            Console.WriteLine("[tool] {0}: {1} พังแล้ว", Name, brokenName);
            Send(new Info { Text = $"{brokenName} พังแล้ว — ต้องคราฟต์อันใหม่" });
        }
        else if (left <= max * 0.2f && left + wear > max * 0.2f)
        {
            // เตือนครั้งเดียวตอนข้ามเส้น 20% ไม่ใช่ทุกครั้งที่ใช้หลังจากนั้น
            Console.WriteLine("[tool] {0}: เครื่องมือเหลือ {1:F0}/{2:F0}", Name, left, max);
            Send(new Info { Text = $"เครื่องมือใกล้พังแล้ว (เหลือ {left:F0}/{max:F0})" });
        }
    }

    /// <summary>บอก client ว่าขาดเครื่องมืออะไร (client มีหน้าต่างของมันเองสำหรับ packet นี้)</summary>
    private void SendToolNeeded(string tag, uint replyOf)
    {
        Send(new ToolNeeded
        {
            RecipeIds = ItemTagData.RecipesMakingTag(tag),
            Skills = null,
            TagNames = ToolNameOf(tag),
            Tags = new Dictionary<string, int> { { tag, 1 } }
        }, replyOf);
    }

    /// <summary>ชื่อไทยของเครื่องมือไว้บอกผู้เล่นว่าต้องมีอะไร</summary>
    private static string ToolNameOf(string tag)
    {
        switch (tag)
        {
            case "axe": return "ขวาน";
            case "axe_onehand_tool": return "ขวานมือเดียว (หรือมีด/ดาบหิน ถ้าสูตรรับ)";
            case "axe_twohand_tool": return "ขวานสองมือ";
            case "hammer_onehand": return "ค้อนมือเดียว";
            case "hammer_twohand": return "ค้อนสองมือ";
            case "knife": return "มีด";
            case "pickaxe": return "อีเต้อ";
            case "shovel": return "พลั่ว";
            case "hammer": return "ค้อน";
            case "sickle": return "เคียว";
            default: return tag;
        }
    }

    private static List<Generator> MakeGenerators(ushort entityType)
    {
        var list = new List<Generator>();
        var bareHands = new Dictionary<string, int> { { "bare_hands", 1 } };
        int level = ServerConfig.ResourceLevel;
        // [TodoList/04] เวลาเก็บตามเกม = effort_standard.collect(level) = 2.5 + (level-1)×0.25 วินาที ทุกชิ้นเท่ากัน
        // (เดิม 1.5+i — ของชิ้นที่ 3 ในต้นเดียวกันช้ากว่าชิ้นแรก 2 เท่าโดยไม่มีเหตุผล) · ปิดสวิตช์ = ตัวเลขเดิม
        CraftingConfig effortCfg = ServerConfig.Current.Crafting;
        bool effortFormula = effortCfg != null && effortCfg.EffortFormula;
        if (NaturalData.Map.TryGetValue(entityType, out NaturalData.GenEntry[] entries))
        {
            for (int i = 0; i < entries.Length; i++)
            {
                float effort = effortFormula ? effortCfg.CollectSeconds(level) : 1f + i;
                list.Add(new Generator
                {
                    Id = entries[i].Prototype,
                    Name = entries[i].Name,
                    Icon = entries[i].Icon,
                    Level = level,
                    Amount = 3 - (i % 2),
                    Effort = effort,
                    Duration = effortFormula ? effort : 1.5f + i,
                    ToolRequirements = ToolRequirementFor(entries[i].Prototype),
                    Enabled = true
                });
            }
            return list;
        }
        // [4 ก.ย. 2026] เดิม fallback เป็น "ใบไม้" อย่างเดียวเสมอ ⇒ เกาะที่ชนิดของธรรมชาติไม่อยู่ใน
        // NaturalData.Map (เช่นเกาะหิมะ) **ทุกต้นทุกก้อนหินให้ใบไม้หมด** — เจ้าของแจ้งเป็นบั๊ก
        // ตอนนี้เดาจากช่วง id แทน: 11xxx=พืช · 12xxx/13xxx/15xxx=หิน/แร่ · 14xxx=ต้นไม้
        float fbEffort = effortFormula ? effortCfg.CollectSeconds(level) : 1f;
        void AddFb(string proto, string name, string icon, System.Collections.Generic.Dictionary<string, int> tools)
        {
            list.Add(new Generator
            {
                Id = proto, Name = name, Icon = icon, Level = level, Amount = 3,
                Effort = fbEffort, Duration = effortFormula ? fbEffort : 1.5f,
                ToolRequirements = tools, Enabled = true
            });
        }
        if (entityType >= 14000 && entityType < 15000)
        {
            AddFb("wood_bough", "กิ่งไม้", "icon_nat_wood_branch", ToolRequirementFor("wood_bough"));
            AddFb("wood_log", "ท่อนไม้", "icon_nat_wood_log", ToolRequirementFor("wood_log"));
            AddFb("leaf", "ใบไม้", "icon_nat_leaf", bareHands);
        }
        else if (entityType >= 12000 && entityType < 14000)
        {
            AddFb("stone", "หิน", "icon_nat_mine_stone", ToolRequirementFor("stone"));
            AddFb("stone_big", "หินก้อนใหญ่", "icon_nat_mine_rock", ToolRequirementFor("stone_big"));
        }
        else if (entityType >= 15000 && entityType < 16000)
        {
            AddFb("stone", "หิน", "icon_nat_mine_stone", ToolRequirementFor("stone"));
            AddFb("stone_big", "หินก้อนใหญ่", "icon_nat_mine_rock", ToolRequirementFor("stone_big"));
        }
        else
        {
            // 11xxx (พืช/พุ่ม) และชนิดที่ไม่รู้จักจริง ๆ
            AddFb("leaf", "ใบไม้", "icon_nat_leaf", bareHands);
            if (entityType >= 11000 && entityType < 12000)
            {
                AddFb("stem", "ลำต้น", "icon_nat_fiber_reed", ToolRequirementFor("stem"));
            }
        }
        return list;
    }

    private void HandleCollect(Collect msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Gathering)
        {
            Console.WriteLine("[feature] ปฏิเสธ {0}: ระบบเก็บของปิดอยู่ในรอบนี้ (Features.Gathering)", Name);
            Send(new Info { Text = "ระบบเก็บของยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        Console.WriteLine("[collect] {0} gen={1} tool={2}", msg.EntityId, msg.GeneratorId, msg.ToolItemId);
        if (Dead)                       // เฟส C รอบ 2: ตายแล้วทำอะไรไม่ได้จนกว่าจะฟื้น
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // แล่ซากสัตว์ — คนละเส้นทางกับของธรรมชาติ (ไม่มี tile ให้ผูก และหมดทีละชิ้นส่วน)
        if (_world.Animals.TryGet(msg.EntityId, out ServerAnimal corpse))
        {
            HandleButchery(corpse, msg, header);
            return;
        }
        // แปลงผักที่โตแล้ว — เก็บเกี่ยวใช้ทางเดียวกับแล่ซาก (หลาย generator หมดทีละอัน)
        if (_world.TryGetFarm(msg.EntityId, out ServerWorld.FarmPlot plot))
        {
            HandleHarvest(plot, msg, header);
            return;
        }
        // GP-09: tile มาจากที่ server จำไว้ตอน Touch ไม่ใช่จาก msg.Tile
        // เดิม client ส่ง Tile อะไรมาก็ได้ → เก็บของที่นี่แต่สั่งลบต้นไม้อีกฟากแมพ
        if (!_world.TryGetNaturalTile(msg.EntityId, out Point2 tile))
        {
            Console.WriteLine("[collect] ปฏิเสธ {0}: ยังไม่ได้แตะ {1} (server ไม่รู้ว่าอยู่ tile ไหน)", Name, msg.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!IsWithinReach(tile))
        {
            Console.WriteLine("[collect] ปฏิเสธ {0}: {1} อยู่ไกลเกินเอื้อมแล้ว", Name, msg.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // GP-03: หักจำนวนทันทีที่ขอ (อะตอมมิกที่ระดับ world) ไม่ใช่ตอนเก็บเสร็จ
        // ถ้าสองคนกดพร้อมกันบนหน่วยสุดท้าย จะมีคนเดียวที่ผ่านตรงนี้ อีกคนได้ Abort
        if (InventoryFull)
        {
            Console.WriteLine("[inventory] {0} กระเป๋าเต็ม เก็บของไม่ได้", Name);
            Send(Aborts.Reason(InventoryFullMessage), header.Seq);
            return;
        }
        // H-6: เพดานงานที่รอเวลาอยู่ต่อผู้เล่น — ไม่เช็คแล้วสแปม packet ยัดคิวโตไม่จำกัดจน main loop ค้าง
        if (_deferred.Count >= MaxPendingActions)
        {
            Console.WriteLine("[collect] ปฏิเสธ {0}: มีงานค้างอยู่ {1} รายการแล้ว", Name, _deferred.Count);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // GP-08b: ต้องมีเครื่องมือที่ generator ขอจริง ๆ (ตัดไม้ต้องมีขวาน · ทุบหินต้องมีอีเต้อ)
        // เช็ค**ก่อน**จอง ไม่งั้นคนที่ไม่มีขวานจะกินหน่วยของคนอื่นทิ้งไปเปล่า ๆ
        // [แก้เอง] 24 ส.ค. 2026 — เช็คก่อนหักสตามินาด้วย (เดิมหักสตามินาไปก่อนเช็คมี/ไม่มีเครื่องมือ —
        // กดเก็บของทั้งที่ไม่มีขวาน/อีเต้อ เสียสตามินาฟรีทุกครั้ง)
        if (!CheckToolBeforeCollect(msg.EntityId, msg.GeneratorId, msg.ToolItemId, header, out string usedTool))
        {
            return;
        }
        // เฟส C: เก็บของกินสตามินา ความล้าสูงยิ่งเปลืองมากขึ้น — หักหลังผ่านเช็คเครื่องมือแล้ว
        IModEventContext? gatherBefore = PluginManager.Instance?.FireEvent("gather.before", this, true, false,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["entity_id"] = msg.EntityId ?? "",
                ["generator_id"] = msg.GeneratorId ?? ""
            });
        if (gatherBefore != null && gatherBefore.IsCancelled)
        {
            Send(new Info { Text = gatherBefore.CancelReason ?? "mod ยกเลิกการเก็บของ" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!TrySpendStamina(StaminaCostCollect, ActionKind.Collect))
        {
            Console.WriteLine("[survival] {0} สตามินาไม่พอสำหรับเก็บของ", Name);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!_world.TryReserveGenerator(msg.EntityId, msg.GeneratorId, out Generator generator, out bool ranOut))
        {
            // [แก้เอง] อีกคนชิงหน่วยสุดท้ายไปก่อนในติ๊กเดียวกัน — คืนสตามินาที่เพิ่งหักไป ไม่งั้นเสียฟรี
            RestoreStamina(StaminaCostCollect, 0f);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // GP-09b: ใช้ Duration ของ generator จริง ๆ (เดิม 2 วิตายตัวทุกชิ้น)
        // แล้วคูณด้วยสกิลหมวดเก็บของ — เวลาที่บอก client กับที่ server หน่วงต้องตรงกัน
        // [TodoList/04] คิดสดจาก config ทุกครั้ง (generator ถูกแคชไว้ใน world — ค่าจะค้างถ้าแก้ config/ResourceLevel ระหว่างรัน)
        CraftingConfig gatherCfg = ServerConfig.Current.Crafting;
        int gatherLevelForEffort = generator.Level > 0 ? generator.Level : ServerConfig.ResourceLevel;
        float gatherBase = gatherCfg != null && gatherCfg.EffortFormula
            ? gatherCfg.CollectSeconds(gatherLevelForEffort)
            : (generator.Duration > 0f ? generator.Duration : 2f);
        float gatherSeconds = Math.Max(0.5f, gatherBase * GatherDurationScale());
        Send(new Messages.Timer { Duration = gatherSeconds }, header.Seq);
        int resourceLevel = generator.Level > 0 ? generator.Level : ServerConfig.ResourceLevel;
        int gatherSkill = Math.Max(1, ProficiencyLevel(Shared.Skill.Category.Gathering));
        bool bonusItem = RollGatherBonus();
        bool greatGather = bonusItem || RollGatherGreatSuccess();
        Generator gatherStamp = generator;
        gatherStamp.Level = ResolveSkillItemLevel(Shared.Skill.Category.Gathering, resourceLevel, greatGather);
        Item item = MakeGatheredItem(gatherStamp);
        Item extra = bonusItem ? MakeGatheredItem(gatherStamp) : default;
        _deferred.Add((Times.UnixTimeNow() + gatherSeconds + 0.1, () =>
        {
            Send(new Collected
            {
                Items = bonusItem ? new[] { item, extra } : new[] { item },
                Result = greatGather ? Result.GreatSuccess : Result.Success,
                ActionInfo = new ActionInfo
                {
                    ActionLevel = gatherSkill,
                    PotentialLevel = resourceLevel,
                    RelatedCategory = Shared.Skill.Category.Gathering,
                    SuccessRatio = 0.85f + SkillRatio(Shared.Skill.Category.Gathering) * 0.14f,
                    RelatedAbility = Shared.Ability.Derived.Gathering
                },
                RanOut = ranOut
            }, header.Seq);
            // GP-03: state เป็นของกลางแล้ว คนอื่นที่เปิดจุดนี้ค้างไว้ต้องรู้ด้วยว่าจำนวนเปลี่ยน
            _world.BroadcastNear(CurrentPosition, new CollectibleChanged { EntityId = msg.EntityId });
            if (ranOut)
            {
                _world.ForgetNaturalTile(msg.EntityId);      // GP-09
                if (_world.Terrain.RemoveNatural(tile.x, tile.y, regrowable: true))
                {
                    _world.MarkDirty();   // GP-07
                    _world.BroadcastNear(new WorldPosition(tile.x * 200f + 100f, tile.y * 200f + 100f), new DisappearEntityOnTile { EntityId = msg.EntityId, Tile = tile });
                    Console.WriteLine("[natural] ran out: tile={0},{1}", tile.x, tile.y);
                }
            }
            lock (_inventory)
            {
                _inventory.Add(item);
                if (bonusItem)
                {
                    _inventory.Add(extra);      // สกิลเก็บของทำให้บางครั้งได้ 2 ชิ้น
                }
            }
            MarkDirty();          // GP-07
            SendInventory();
            PluginManager.Instance?.FireEvent("gather.completed", this, false, true,
                new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["entity_id"] = msg.EntityId ?? "",
                    ["generator_id"] = msg.GeneratorId ?? "",
                    ["output_count"] = (bonusItem ? 2 : 1).ToString()
                });
            PluginManager.Instance?.FireEvent("inventory.added", this, false, true);
            GainExpForGather();
            // บั๊ก #7 — ตักโคลน/ดินเหนียวแล้วต้องเปื้อน (ข้อมูลเกมมีสถานะ dirty อยู่แล้ว)
            if ((item.Prototype ?? string.Empty).StartsWith("clay", StringComparison.Ordinal))
            {
                MakeDirty();
            }
            NoteGatheredItem(item.Prototype);          // เควสที่เจาะจงของ เช่น "เก็บท่อนซุง 10 อัน"
            if (bonusItem)
            {
                NoteGatheredItem(extra.Prototype);     // สกิลเก็บของทำให้ได้ 2 ชิ้น — นับทั้งคู่
            }
            WearTool(usedTool);   // ขวาน/อีเต้อสึกก็ต่อเมื่อได้ของจริง
        }));
    }

    /// <summary>
    /// แล่ซาก 1 ชิ้นส่วน — โครงเดียวกับเก็บของธรรมชาติ ต่างกัน 3 อย่าง
    ///   1. ระยะคิดจากตำแหน่งซากตรง ๆ (ซากไม่ได้ผูกกับ tile)
    ///   2. ชิ้นส่วนหนึ่งหมดแล้วยังแล่ชิ้นอื่นต่อได้ (TryReserveCorpsePart)
    ///   3. แล่หมดทั้งตัว = ซากหายจากโลกทันที ไม่ต้องรอครบเวลา
    /// </summary>
    private void HandleButchery(ServerAnimal corpse, Collect msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Butchery)
        {
            Send(new Info { Text = "ระบบแล่ซากยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (corpse.IsAlive)
        {
            Console.WriteLine("[butchery] ปฏิเสธ {0}: {1} ยังไม่ตาย", Name, corpse.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        WorldPosition me = CurrentPosition;
        float dx = corpse.Position.x - me.x;
        float dy = corpse.Position.y - me.y;
        if (dx * dx + dy * dy > ButcheryRange * ButcheryRange)
        {
            Console.WriteLine("[butchery] ปฏิเสธ {0}: ซาก {1} อยู่ไกลเกินเอื้อม", Name, corpse.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (InventoryFull)
        {
            Console.WriteLine("[inventory] {0} กระเป๋าเต็ม แล่เนื้อไม่ได้", Name);
            Send(Aborts.Reason(InventoryFullMessage), header.Seq);
            return;
        }
        // H-6: เพดานงานที่รอเวลาอยู่ต่อผู้เล่น — กันสแปม packet ยัดคิวโตไม่จำกัด
        if (_deferred.Count >= MaxPendingActions)
        {
            Console.WriteLine("[butchery] ปฏิเสธ {0}: มีงานค้างอยู่ {1} รายการแล้ว", Name, _deferred.Count);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // ต้องมีมีดถึงจะแล่ได้ (เช็คก่อนจอง ไม่งั้นคนไม่มีมีดกินชิ้นส่วนของคนอื่นทิ้งเปล่า ๆ)
        // [แก้เอง] 24 ส.ค. 2026 — เช็คก่อนหักสตามินาด้วย (เดิมหักไปก่อนเช็คมีมีดไหม เสียสตามินาฟรี)
        if (!CheckToolBeforeCollect(corpse.EntityId, msg.GeneratorId, msg.ToolItemId, header, out string usedKnife))
        {
            return;
        }
        IModEventContext? beforeButchery = PluginManager.Instance?.FireEvent("butchery.before", this, true, false,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["entity_id"] = corpse.EntityId, ["generator_id"] = msg.GeneratorId ?? "", ["tool_item_id"] = msg.ToolItemId ?? "" });
        if (beforeButchery?.IsCancelled == true)
        { Send(new Info { Text = beforeButchery.CancelReason ?? "การแล่ถูกยกเลิกโดยม็อด" }, header.Seq); Send(Aborts.Reason(), header.Seq); return; }
        if (!TrySpendStamina(StaminaCostCollect, ActionKind.Collect))
        {
            Console.WriteLine("[survival] {0} สตามินาไม่พอสำหรับแล่เนื้อ", Name);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // จองก่อนเสมอ (GP-03) — สองคนแล่ซากเดียวกันพร้อมกันจะได้ไม่ได้ของซ้ำ
        if (!_world.TryReserveCorpsePart(corpse.EntityId, msg.GeneratorId, out Generator part, out bool emptied))
        {
            // [แก้เอง] อีกคนชิงชิ้นส่วนสุดท้ายไปก่อน — คืนสตามินาที่เพิ่งหักไป
            RestoreStamina(StaminaCostCollect, 0f);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        float duration = Math.Max(0.5f, (part.Duration > 0f ? part.Duration : 2f) * ButcheryDurationScale());
        Send(new Messages.Timer { Duration = duration }, header.Seq);
        int meatBase = part.Level > 0 ? part.Level : ServerConfig.ResourceLevel;
        int butcherSkill = Math.Max(1, ProficiencyLevel(Shared.Skill.Category.Butchery));
        bool bonusPart = RollButcheryBonus();
        bool greatButcher = bonusPart || RollButcheryGreatSuccess();
        Generator meatStamp = part;
        meatStamp.Level = ResolveSkillItemLevel(Shared.Skill.Category.Butchery, meatBase, greatButcher);
        Item item = MakeGatheredItem(meatStamp);
        Item extraPart = bonusPart ? MakeGatheredItem(meatStamp) : default;
        _deferred.Add((Times.UnixTimeNow() + duration + 0.1, () =>
        {
            Send(new Collected
            {
                Items = bonusPart ? new[] { item, extraPart } : new[] { item },
                Result = greatButcher ? Result.GreatSuccess : Result.Success,
                ActionInfo = new ActionInfo
                {
                    ActionLevel = butcherSkill,
                    PotentialLevel = meatBase,
                    RelatedCategory = Shared.Skill.Category.Butchery,
                    SuccessRatio = 0.85f + SkillRatio(Shared.Skill.Category.Butchery) * 0.14f,
                    RelatedAbility = Shared.Ability.Derived.Invalid
                },
                RanOut = emptied
            }, header.Seq);
            _world.BroadcastToViewers(corpse.EntityId, new CollectibleChanged { EntityId = corpse.EntityId });
            if (emptied)
            {
                // แล่จนไม่เหลืออะไร — เอาซากออกเลย ดูสมเหตุสมผลกว่าปล่อยให้ซากเปล่านอนรอหมดเวลา
                Console.WriteLine("[butchery] {0} แล่ {1} จนหมดตัว — ซากหายไป", Name, corpse.EntityId);
                _world.Animals.Remove(corpse.EntityId);
            }
            lock (_inventory)
            {
                _inventory.Add(item);
                if (bonusPart)
                {
                    _inventory.Add(extraPart);  // สกิลชำแหละทำให้บางครั้งได้ 2 ชิ้น
                }
            }
            MarkDirty();          // GP-07
            SendInventory();
            PluginManager.Instance?.FireEvent("butchery.completed", this, false, true);
            PluginManager.Instance?.FireEvent("inventory.added", this, false, true);
            GainExpForButchery();
            WearTool(usedKnife);  // มีดสึกก็ต่อเมื่อแล่ได้ของจริง
            Console.WriteLine("[butchery] {0} ได้ {1} จากซาก {2}", Name, part.Name, corpse.EntityId);
        }));
    }

    private static Item MakeGatheredItem(Generator generator)
    {
        int level = generator.Level > 0 ? generator.Level : ServerConfig.ResourceLevel;
        return new Item
        {
            Id = Guid.NewGuid().ToString(),
            Name = generator.Name,
            Description = generator.Name,
            Icon = generator.Icon,
            SubIcon = null,
            Prototype = generator.Id,
            Level = level,
            OriginalLevel = level,
            // 🐛 **ตัวที่ทำให้ "มีเนื้อ 10 ชิ้นแต่คราฟต์ไม่ได้"** — เดิมเป็น 0
            //
            // สูตรที่มี `deduct_modifiable_count: true` (สูตรทำอาหาร/แปรรูปแทบทั้งหมด)
            // ช่อง "base" ของมันจะกลายเป็น `RecipeSlot.Type.ModifyBase` ฝั่ง client
            // แล้ว `RecipeSlot.IsSuitableItem` เช็คเพิ่มว่า **`itemData.ModifiableCount > 0`**
            // ⇒ ของที่เราส่งไป ModifiableCount = 0 ถูกกรองทิ้งหมด ช่องเลยขึ้นว่า "ไม่มีของ"
            // ทั้งที่มีอยู่เต็มกระเป๋า และ **packet ไม่เคยถูกส่งมาถึง server เลย** (client กันไว้ก่อน)
            //
            // ช่องที่ใช้ `required_tags` (เช่นช่อง "น้ำ" ของ boiled_meat) เป็น General
            // จึงผ่านปกติ — นี่คือเหตุผลที่บางช่องมีของบางช่องว่าง
            ModifiableCount = 1,
            ModifiedCount = 0,
            Size = 1,
            // ปกติของที่เก็บได้ไม่ใช่เครื่องมือ (MaxFor คืน 0 ⇒ หลอด 1/1 เหมือนเดิม)
            // แต่ cheat add axe/knife ก็ผ่านทางนี้ จึงต้องเติมความทนทานให้ด้วย
            Durability = ToolDurability.MakeGauge(ToolDurability.MaxFor(generator.Id), ToolDurability.MaxFor(generator.Id)),
            // [4 ก.ย. 2026] สีจริงจาก prototype_data — เดิม FFFFFF (ขาว)
            ColorR = GameData.ItemColorOrWhite(generator.Id).R,
            ColorG = GameData.ItemColorOrWhite(generator.Id).G,
            ColorB = GameData.ItemColorOrWhite(generator.Id).B,
            Unstable = false,
            RepairRequirement = ToolDurability.RepairRequirementFor(generator.Id),
            FounderId = null,
            FounderCategory = null,
            Tags = ItemTagData.For(generator.Id),
            TagModifications = null,
            // แนบช่องที่ใส่ได้ไปด้วย ไม่งั้น client กดใส่อุปกรณ์ไม่ได้ (ดู EquipData.PerformanceFor)
            Performance = EquipData.PerformanceFor(generator.Id),
            Ext = null,
            CollectibleId = null,
            GeneratorId = generator.Id,
            EmotionalMotions = null,
            PioneerCost = 0f
        };
    }

    private void HandleGetCollectible(GetCollectible msg, PacketHeader header)
    {
        // GP-03: อ่านจาก world (คืน null ถ้ายังไม่มีใครแตะจุดนี้)
        Send(new Collectible
        {
            EntityId = msg.EntityId,
            CollectibleId = null,
            Size = null,
            Generators = _world.PeekGenerators(msg.EntityId),
            CriticalGenerator = null
        }, header.Seq);
    }
}
