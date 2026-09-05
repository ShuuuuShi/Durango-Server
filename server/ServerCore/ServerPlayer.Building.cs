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

// ServerPlayer.Building — ดูรายละเอียดที่ docs/server/ServerPlayer.Building.md

public partial class ServerPlayer
{
    /// <summary>สิ่งปลูกสร้างที่กำลังนับเวลาสร้างอยู่ — กันยิง BuildArtifact ซ้ำระหว่างรอ</summary>
    private readonly HashSet<string> _buildingNow = new HashSet<string>(StringComparer.Ordinal);


    private static Item MakeCapsuleItem(string prototype, string name, string icon)
    {
        string blueprintId = prototype.StartsWith("capsulated_") ? prototype.Substring("capsulated_".Length) : prototype;
        return new Item
        {
            Id = Guid.NewGuid().ToString(),
            Name = name,
            Description = name,
            Icon = icon,
            SubIcon = null,
            Prototype = prototype,
            Level = 1,
            OriginalLevel = 1,
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
            Durability = new Gauge(1f, 0f, new[] { new GaugeNode { Time = 0.0, Value = 1f } }),
            ColorR = "FFFFFF",
            ColorG = "FFFFFF",
            ColorB = "FFFFFF",
            Unstable = false,
            RepairRequirement = null,
            FounderId = null,
            FounderCategory = null,
            Tags = ItemTagData.For(prototype),
            TagModifications = null,
            Performance = null,
            Ext = new ArtifactCapsule
            {
                EntityId = null,
                BlueprintId = blueprintId,
                ArtifactLevel = 1,
                Tags = null,
                Performance = null,
                Display = default,
                State = default,
                LookNames = null,
                OccupySize = new Point2(1, 1)
            },
            CollectibleId = null,
            GeneratorId = null,
            EmotionalMotions = null,
            PioneerCost = 0f
        };
    }

    /// <summary>H-7: สร้างได้คนละกี่ชิ้น (กันบอทถมทั้งเกาะจนคนใหม่เข้าเกมไม่ได้)</summary>
    private const int MaxArtifactsPerPlayer = 40;

    private bool AllowFreeBuild => ServerConfig.Current.CraftMenu?.AllowFreeBuild ?? false;

    private void HandleOccupyArtifactSite(OccupyArtifactSite msg, PacketHeader header)
    {
        if (Dead)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // H-7: เดิมไม่ตรวจอะไรเลย — ปล่อยบอทค้างคืนได้บ้านหลายหมื่นหลัง
        // ทุกหลังถูกเซฟถาวรและถูกส่งให้ "ทุกคนที่เข้าเกม" ⇒ บัฟเฟอร์ส่งล้น = คนใหม่เข้าไม่ได้อีกเลย
        if (msg.Tile.x < 0 || msg.Tile.y < 0
            || msg.Tile.x >= _world.Terrain.Width || msg.Tile.y >= _world.Terrain.Height)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: tile {1},{2} อยู่นอกแมพ", Name, msg.Tile.x, msg.Tile.y);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!IsWithinReach(msg.Tile))
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: tile {1},{2} ไกลเกินเอื้อม", Name, msg.Tile.x, msg.Tile.y);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // สิทธิ์ที่ดิน: ห้ามจองที่สร้างบนแปลงคนอื่นถ้าเจ้าของไม่ได้ให้สิทธิ์ Occupy
        if (!RejectIfLandLocked(msg.Tile, Shared.Estate.AccessRights.Occupy, "จองที่สร้าง", header))
        {
            return;
        }
        // ⚠️ ต้องรู้ขนาดจริงก่อนถึงจะเช็คพื้นที่ทับซ้อนได้ — ดูหมายเหตุที่ ResolveBlueprintSize
        int mine = _world.CountArtifactsOf(EntityId);
        if (mine >= MaxArtifactsPerPlayer)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: สร้างครบเพดานแล้ว ({1} ชิ้น)", Name, mine);
            Send(new Info { Text = $"สร้างได้สูงสุด {MaxArtifactsPerPlayer} ชิ้นต่อคน — ทุบของเก่าก่อน" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        IModEventContext? placeBefore = PluginManager.Instance?.FireEvent("building.before_place", this, true, false,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["blueprint_id"] = msg.BlueprintId ?? "",
                ["tile_x"] = msg.Tile.x.ToString(),
                ["tile_y"] = msg.Tile.y.ToString()
            });
        if (placeBefore != null && placeBefore.IsCancelled)
        {
            Send(new Info { Text = placeBefore.CancelReason ?? "mod ยกเลิกการวางสิ่งปลูกสร้าง" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // สตามินาจองที่สร้าง = สูตรจริงของเกม (constants.json → build/site_selection/energy)
        //   1 + (พื้นที่เป็นช่อง × 2) — ของชิ้นเล็กถูกกว่าของชิ้นใหญ่ ไม่ใช่ 8 เท่ากันหมดแบบเดิม
        Point2 siteSize = ResolveBlueprintSize(msg.BlueprintId, new Point2(1, 1));
        float siteArea = Math.Max(1, siteSize.x) * Math.Max(1, siteSize.y);
        float siteEnergy = ServerConfig.Current.Survival.BuildSiteEnergyBase
                         + ServerConfig.Current.Survival.BuildSiteEnergyPerArea * siteArea;
        if (!TrySpendStamina(siteEnergy, ActionKind.Build))
        {
            Console.WriteLine("[survival] {0} สตามินาไม่พอสำหรับจองที่สร้าง (ต้องใช้ {1})", Name, siteEnergy);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // [แก้เอง] 25 ส.ค. 2026 — สิ่งก่อสร้าง event (คริสต์มาส/ฮาโลวีน ฯลฯ) วางได้เฉพาะ admin
        // handler นี้เป็นเส้นทาง "จองที่สร้าง" ที่ไม่เคยเช็ค unlock/recipe อะไรเลย (client ส่ง
        // BlueprintId มาตรง ๆ ก็วางได้ทันทีถ้ามีอยู่ใน BlueprintType) ต้องกันตรงนี้ถึงจะจริง
        if (RecipeData.IsEventBlueprint(msg.BlueprintId) && !IsAdmin)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: '{1}' เป็นของอีเวนต์ — admin เท่านั้น", Name, msg.BlueprintId);
            RestoreStamina(StaminaCostBuild, 0f);
            Send(new Info { Text = "สิ่งก่อสร้างนี้เป็นของอีเวนต์ — ใช้ได้แค่แอดมิน" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // [แก้เอง] 25 ส.ค. 2026 (รอบ 3) — เอาเกณฑ์ความสามารถที่ประมาณเอาเอง (BlueprintGateData) ออก
        // เปลี่ยนมาเช็คของจริง: BlueprintId นี้ต้องอยู่ใน unlocked set จริง (AlwaysBlueprints หรือ
        // เรียนสกิลมาแล้ว) — เหมือนที่แก้ HandleCraft
        if (Array.IndexOf(UnlockedBlueprints(), msg.BlueprintId) < 0)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: '{1}' ยังไม่ปลดล็อก (ต้องเรียนสกิลที่เกี่ยวข้องก่อน)", Name, msg.BlueprintId);
            RestoreStamina(StaminaCostBuild, 0f);
            Send(new Info { Text = "สิ่งก่อสร้างนี้ยังไม่ปลดล็อก — เรียนสกิลที่เกี่ยวข้องก่อน" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!AllowFreeBuild
            && BlueprintRequirements.TryGet(msg.BlueprintId ?? string.Empty, out BlueprintRequirements.Slot[] requestedSlots)
            && requestedSlots.Length == 0)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: '{1}' เป็นแบบก่อสร้างฟรีและปิดอยู่", Name, msg.BlueprintId);
            RestoreStamina(StaminaCostBuild, 0f);
            Send(new Info { Text = "การสร้างสิ่งก่อสร้างฟรีถูกปิดอยู่ — ต้องใช้แบบที่มีวัตถุดิบ" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        string entityId = Guid.NewGuid().ToString();
        ushort entityType = 0;
        if (!RecipeData.BlueprintType.TryGetValue(msg.BlueprintId ?? "", out entityType))
        {
            Console.WriteLine("[build] occupy FAILED: unknown blueprint '{0}'", msg.BlueprintId);
            RestoreStamina(StaminaCostBuild, 0f);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        Point2 size = ResolveBlueprintSize(msg.BlueprintId, msg.Size);

        // 🐛 เดิมเช็คแค่ `HasArtifactAt(msg.Tile)` = **tile มุมเดียว** ⇒ ของ 2×2 วางเยื้อง 1 ช่อง
        //    จะผ่านทั้งที่ทับของเดิมอยู่ครึ่งหนึ่ง — วางบ้านซ้อนกันได้จริง
        if (_world.HasArtifactOverlapping(msg.Tile, size))
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: พื้นที่ {1},{2} ขนาด {3}x{4} ทับสิ่งปลูกสร้างเดิม",
                Name, msg.Tile.x, msg.Tile.y, size.x, size.y);
            RestoreStamina(StaminaCostBuild, 0f);
            Send(new Info { Text = "ตรงนี้มีสิ่งปลูกสร้างอยู่แล้ว" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        Console.WriteLine("[build] occupy {0} type={1} blueprint={2} tile={3},{4} size={5},{6}", entityId, entityType, msg.BlueprintId, msg.Tile.x, msg.Tile.y, size.x, size.y);
        AppearArtifact artifact = MakeArtifact(entityId, entityType, msg.Tile, size, msg.Rotation, msg.Floor, msg.Stories ?? 1, msg.BlueprintId);
        // GP-04: จำไว้ในโลกก่อน แล้วค่อย broadcast — คนที่เข้ามาทีหลังจะได้เห็นด้วย
        _world.AddArtifact(artifact, msg.BlueprintId);
        _world.AnnounceArtifact(artifact);
        PluginManager.Instance?.FireEvent("building.placed", this, false, true,
            new Dictionary<string, string>(StringComparer.Ordinal)
            {
                ["blueprint_id"] = msg.BlueprintId ?? "",
                ["entity_id"] = entityId,
                ["tile_x"] = msg.Tile.x.ToString(),
                ["tile_y"] = msg.Tile.y.ToString()
            });
        Send(new Messages.Timer { Duration = 2f }, header.Seq);
        Send(new Occupied
        {
            EntityId = entityId,
            TileX = msg.Tile.x,
            TileY = msg.Tile.y,
            Floor = msg.Floor
        }, header.Seq);
    }

    /// <summary>
    /// ขนาดของสิ่งปลูกสร้าง — **เชื่อข้อมูลเกมก่อนเสมอ ไม่เชื่อ client**
    ///
    /// 🐛 เดิมใช้ `msg.Size` ที่ client ส่งมาตรง ๆ (fallback ไปตารางเฉพาะตอนเป็น 0)
    ///    ⇒ ยิง `Size = 200,200` มาได้ · ของชิ้นเดียวจะ **จองพื้นที่ 40,000 tile**
    ///    ทำให้คนอื่นสร้างอะไรไม่ได้ทั้งย่าน โดยเสียโควตาตัวเองแค่ 1 จาก 40 ชิ้น
    ///
    /// ตอนนี้: มีในตารางข้อมูลเกม → ใช้ของตาราง · ไม่มี → ใช้ของ client แต่ตัดไม่ให้เกิน MaxArtifactSize
    /// </summary>
    private static Point2 ResolveBlueprintSize(string blueprintId, Point2 requested)
    {
        if (RecipeData.BlueprintSize.TryGetValue(blueprintId ?? string.Empty, out var bp) && bp.x > 0 && bp.y > 0)
        {
            return new Point2(bp.x, bp.y);
        }
        int x = requested.x <= 0 ? 1 : Math.Min(requested.x, MaxArtifactSize);
        int y = requested.y <= 0 ? 1 : Math.Min(requested.y, MaxArtifactSize);
        return new Point2(x, y);
    }

    /// <summary>ขนาดสูงสุดที่ยอมให้ client กำหนดเอง (ใช้เฉพาะ blueprint ที่ไม่มีในตาราง)</summary>
    private const int MaxArtifactSize = 8;

    // GP-07: ตัวสร้างจริงย้ายไป ArtifactFactory (static) เพราะตอนโหลดเซฟกลับมา
    // ServerWorld ต้องสร้าง artifact เองโดยไม่มี ServerPlayer ให้อ้างอิง
    private AppearArtifact MakeArtifact(string entityId, ushort entityType, Point2 tile, Point2 size, Rotation rotation, int? floor, int stories, string blueprintId = null)
    {
        return ArtifactFactory.Make(EntityId, entityId, entityType, tile, size, rotation, floor, stories, blueprintId);
    }

	private void HandlePlaceCapsulatedArtifact(PlaceCapsulatedArtifact msg, PacketHeader header)
	{
		if (!ServerConfig.Current.Features.Building)
        {
            RejectFeatureDisabled("Building", "PlaceCapsulatedArtifact", "ระบบก่อสร้างยังไม่เปิดในรอบนี้", header);
            return;
        }
        if (Dead || IsItemLocked(msg.ItemId))
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        string proto = null;
        lock (_inventory)
        {
            int idx = _inventory.FindIndex(it => it.Id == msg.ItemId);
            if (idx >= 0)
            {
                proto = _inventory[idx].Prototype;
            }
        }
        if (proto == null)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // สิทธิ์ที่ดิน: วางของสำเร็จรูปบนแปลงคนอื่นก็ต้องมีสิทธิ์ Occupy เหมือนการจองที่สร้าง
        if (!RejectIfLandLocked(msg.Tile, Shared.Estate.AccessRights.Occupy, "วางสิ่งปลูกสร้าง", header))
        {
            return;
        }
        string blueprintId = proto.StartsWith("capsulated_") ? proto.Substring("capsulated_".Length) : proto;
        if (!RecipeData.BlueprintType.TryGetValue(blueprintId, out ushort entityType))
        {
            Console.WriteLine("[build] place capsule FAILED: unknown blueprint '{0}' from {1}", blueprintId, proto);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!AllowFreeBuild)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: วางแคปซูล '{1}' แบบไม่ใช้วัสดุถูกปิดอยู่", Name, blueprintId);
            Send(new Info { Text = "การวางสิ่งก่อสร้างแบบไม่ใช้วัตถุดิบถูกปิดอยู่ — ต้องสร้างผ่านแบบและใส่วัตถุดิบ" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // [แก้เอง] 25 ส.ค. 2026 — กันซ้ำอีกชั้นเผื่อมีทางได้แคปซูล event มาโดยไม่ผ่านการคราฟ (เช่น
        // เก็บจากพื้น/รับจากคนอื่น) ตัวจริงกันไว้ที่ HandleCraft แล้ว แต่จุดวางก็ควรกันด้วยเหมือนกัน
        if (RecipeData.IsEventBlueprint(blueprintId) && !IsAdmin)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: แคปซูล '{1}' เป็นของอีเวนต์ — admin เท่านั้น", Name, blueprintId);
            Send(new Info { Text = "สิ่งก่อสร้างนี้เป็นของอีเวนต์ — ใช้ได้แค่แอดมิน" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        Item capsule;
        lock (_inventory)
        {
            int idx = _inventory.FindIndex(it => it.Id == msg.ItemId && it.Prototype == proto);
            if (idx < 0)
            {
                Send(Aborts.Reason(), header.Seq);
                return;
            }
            capsule = _inventory[idx];
        }
		Point2 size = new Point2(1, 1);
		if (RecipeData.BlueprintSize.TryGetValue(blueprintId, out var bpSize))
		{
			size = new Point2(bpSize.x, bpSize.y);
		}
		if (msg.Tile.x < 0 || msg.Tile.y < 0
			|| size.x <= 0 || size.y <= 0
			|| msg.Tile.x > _world.Terrain.Width - size.x
			|| msg.Tile.y > _world.Terrain.Height - size.y
			|| !IsWithinReach(msg.Tile)
			|| _world.HasArtifactOverlapping(msg.Tile, size)
			|| _world.CountArtifactsOf(EntityId) >= MaxArtifactsPerPlayer)
		{
			Console.WriteLine("[build] ปฏิเสธ {0}: ตำแหน่งแคปซูลไม่ถูกต้องหรือเกินสิทธิ์", Name);
            lock (_inventory)
            {
                _inventory.Add(capsule);
            }
			Send(Aborts.Reason(), header.Seq);
			return;
		}
        string entityId = Guid.NewGuid().ToString();
        lock (_inventory)
        {
            int idx = _inventory.FindIndex(it => it.Id == msg.ItemId && it.Prototype == proto);
            if (idx < 0)
            {
                Send(Aborts.Reason(), header.Seq);
                return;
            }
            _inventory.RemoveAt(idx);
            ForgetInventoryItem(msg.ItemId);
        }
        Console.WriteLine("[build] place capsule {0} (proto={1}) type={2} tile={3},{4}", entityId, proto, entityType, msg.Tile.x, msg.Tile.y);
        // ของที่อยู่ในแคปซูลคือ "ของสำเร็จรูป" — วางแล้วใช้ได้เลย ไม่ต้องเอาวัสดุมาสร้างซ้ำ
        // 🐛 เดิมวางออกมาเป็น Occupied (= แค่จองพื้นที่) ⇒ กองไฟที่วางจากแคปซูลใช้เป็นโต๊ะคราฟต์ไม่ได้
        AppearArtifact placed = ArtifactFactory.Make(EntityId, entityId, entityType, msg.Tile, size,
            msg.Rotation, msg.Floor, 1, blueprintId, BuildingState.Completed);
        // GP-04
        _world.AddArtifact(placed, blueprintId);
        _world.AnnounceArtifact(placed);
        MarkDirty();              // GP-07 — ของออกจากกระเป๋าไปแล้ว
        Send(new Messages.Timer { Duration = 2f }, header.Seq);
        SendInventory();
    }

    private bool TryGetBuildSlots(string entityId, out BlueprintRequirements.Slot[] slots, out string reason)
    {
        slots = null;
        reason = null;
        if (!_world.TryGetArtifactBlueprint(entityId, out string blueprintId) || string.IsNullOrEmpty(blueprintId))
        {
            reason = "ไม่รู้ชนิดของสิ่งปลูกสร้างนี้";
            return false;
        }
        if (!BlueprintRequirements.TryGet(blueprintId, out slots))
        {
            reason = $"ไม่มีข้อมูลวัตถุดิบของ {blueprintId}";
            return false;
        }
        return true;
    }

    private bool ValidateBuildingDeposit(BlueprintRequirements.Slot[] slots, Dictionary<string, List<Item>> reserved,
        Dictionary<string, string[]> request, out List<string> itemIds, out string reason)
    {
        itemIds = new List<string>();
        reason = null;
        if (request == null || request.Count == 0)
        {
            reason = "ไม่ได้เลือกวัตถุดิบ";
            return false;
        }

        var slotById = new Dictionary<string, BlueprintRequirements.Slot>();
        for (int i = 0; i < slots.Length; i++) slotById[slots[i].Id] = slots[i];
        foreach (var pair in request)
        {
            if (!slotById.ContainsKey(pair.Key))
            {
                reason = $"ไม่มีช่อง '{pair.Key}' ในแบบก่อสร้าง";
                return false;
            }
        }

        var used = new HashSet<string>(StringComparer.Ordinal);
        if (reserved != null)
            foreach (var pair in reserved)
                foreach (Item item in pair.Value)
                    used.Add(item.Id);

        foreach (var pair in request)
        {
            BlueprintRequirements.Slot slot = slotById[pair.Key];
            string[] given = pair.Value ?? Array.Empty<string>();
            int existing = reserved != null && reserved.TryGetValue(pair.Key, out List<Item> old) ? old.Count : 0;
            if (existing + given.Length > slot.Max)
            {
                reason = $"ช่อง '{slot.Id}' ใส่ได้มากสุด {slot.Max} ชิ้น";
                return false;
            }
            for (int i = 0; i < given.Length; i++)
            {
                string id = given[i];
                if (string.IsNullOrEmpty(id) || !used.Add(id))
                {
                    reason = $"ไอเทม {id ?? "(ว่าง)"} ถูกใช้ซ้ำ";
                    return false;
                }
                if (_equippedItems.ContainsValue(id) || IsItemLocked(id))
                {
                    reason = $"ไอเทม {id} ถูกใช้งานหรือล็อกอยู่";
                    return false;
                }
                Item item;
                lock (_inventory)
                {
                    int index = _inventory.FindIndex(x => x.Id == id);
                    if (index < 0)
                    {
                        reason = $"ไม่มีไอเทม {id} อยู่ในกระเป๋า";
                        return false;
                    }
                    item = _inventory[index];
                }
                if (!MatchesAny(item.Prototype, slot.Tags) || !MatchesAny(item.Prototype, slot.Materials))
                {
                    reason = $"ไอเทม {item.Prototype} ไม่ตรงกับช่อง '{slot.Id}'";
                    return false;
                }
                itemIds.Add(id);
            }
        }
        return true;
    }

    private static bool AreBuildSlotsComplete(BlueprintRequirements.Slot[] slots, Dictionary<string, List<Item>> reserved)
    {
        for (int i = 0; i < slots.Length; i++)
        {
            int count = reserved != null && reserved.TryGetValue(slots[i].Id, out List<Item> items) ? items.Count : 0;
            if (count < slots[i].Min || count > slots[i].Max) return false;
        }
        return true;
    }

    private void HandlePutMaterials(PutMaterialsIntoArtifact msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Building)
        {
            RejectFeatureDisabled("Building", "PutMaterials", "ระบบก่อสร้างยังไม่เปิดในรอบนี้", header);
            return;
        }
        if (Dead)
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!_world.TryGetArtifact(msg.EntityId, out AppearArtifact artifact))
        {
            Console.WriteLine("[build] PutMaterials ปฏิเสธ {0}: ไม่มีสิ่งปลูกสร้าง {1}", Name, msg.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!CanModifyArtifact(artifact))
        {
            Console.WriteLine("[build] PutMaterials ปฏิเสธ {0}: ไม่ใช่เจ้าของ {1}", Name, msg.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (artifact.States.BuildingState != BuildingState.Occupied)
        {
            Send(new Info { Text = "สิ่งปลูกสร้างนี้สร้างเสร็จแล้ว — ไม่ต้องใส่วัสดุ" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!IsWithinReach(artifact.Tile))
        {
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        List<string> itemIds = null;
        string reason = null;
        bool hasSlots = TryGetBuildSlots(msg.EntityId, out BlueprintRequirements.Slot[] slots, out string slotsReason);
        bool validDeposit = hasSlots && ValidateBuildingDeposit(slots, _world.GetArtifactMaterials(msg.EntityId), msg.Materials,
            out itemIds, out reason);
        if (!validDeposit)
        {
            string message = slotsReason ?? reason ?? "วัตถุดิบไม่ถูกต้อง";
            Console.WriteLine("[build] PutMaterials ปฏิเสธ {0}: {1}", Name, message);
            Send(new Info { Text = message }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        var deposits = new Dictionary<string, List<Item>>();
        lock (_inventory)
        {
            // ValidateBuildingDeposit saw a snapshot; verify every ID again before any removal.
            foreach (var pair in msg.Materials)
            {
                var items = new List<Item>();
                foreach (string id in pair.Value ?? Array.Empty<string>())
                {
                    int index = _inventory.FindIndex(x => x.Id == id);
                    if (index < 0)
                    {
                        Send(Aborts.Reason(), header.Seq);
                        return;
                    }
                    items.Add(_inventory[index]);
                }
                if (items.Count > 0) deposits[pair.Key] = items;
            }
            foreach (string id in itemIds)
            {
                int index = _inventory.FindIndex(x => x.Id == id);
                if (index >= 0) _inventory.RemoveAt(index);
            }
        }
        var maximums = new Dictionary<string, int>();
        for (int i = 0; i < slots.Length; i++) maximums[slots[i].Id] = slots[i].Max;
        if (!_world.TryReserveArtifactMaterials(msg.EntityId, deposits, maximums))
        {
            lock (_inventory)
            {
                foreach (var pair in deposits) _inventory.AddRange(pair.Value);
            }
            Send(new Info { Text = "วัสดุในช่องนี้ถูกเติมครบโดยผู้เล่นคนอื่นแล้ว" }, header.Seq);
            SendInventory();
            Send(Aborts.Reason(), header.Seq);
            return;
        }

        Console.WriteLine("[build] PutMaterials {0} → {1}: {2} slots", Name, msg.EntityId, deposits.Count);
        MarkDirty();
        SendInventory();
        Send(default(OK), header.Seq);
    }

    private void HandleBuildArtifact(BuildArtifact msg, PacketHeader header)
    {
        if (!ServerConfig.Current.Features.Building)
        {
            Console.WriteLine("[feature] ปฏิเสธ {0}: ระบบก่อสร้างปิดอยู่ในรอบนี้ (Features.Building)", Name);
            Send(new Info { Text = "ระบบก่อสร้างยังไม่เปิดในรอบนี้" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // H-6: เดิมไม่ตรวจอะไรเลย ยิงรัว ๆ ได้ไม่จำกัด → _deferred โตไม่หยุด
        // แล้วอีก 2.1 วิ ทุกงานยิง broadcast 2 packet คูณจำนวนผู้เล่น = main loop ค้าง
        // แถมส่ง id ของบ้านคนอื่นมาก็เปลี่ยนสถานะบ้านเขาเป็น Built ได้ฟรี ๆ
        if (!_world.TryGetArtifact(msg.EntityId, out AppearArtifact target))
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: ไม่มีสิ่งปลูกสร้าง {1}", Name, msg.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!CanModifyArtifact(target))
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: ไม่ใช่เจ้าของ {1}", Name, msg.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // 🐛 **ช่องปั๊มที่หนักที่สุดของระบบนี้** — เดิมไม่เช็คสถานะเลย
        //    สร้างกองไฟ 1 อันแล้วยิง BuildArtifact ใส่ตัวเดิมรัว ๆ ได้ไม่จำกัด
        //    แต่ละครั้งได้ exp ก่อสร้าง + ความชำนาญ + **ความคืบหน้าเควส "สร้าง N อย่าง"**
        //    (เควสต่อแพก็ปั๊มได้ด้วยวิธีเดียวกัน) — ต้องสร้างได้เฉพาะของที่ยัง "จองที่ไว้เฉย ๆ"
        if (target.States.BuildingState != BuildingState.Occupied)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: {1} สร้างเสร็จไปแล้ว (สถานะ {2})",
                Name, msg.EntityId, target.States.BuildingState);
            Send(new Info { Text = "สิ่งปลูกสร้างนี้สร้างเสร็จแล้ว" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!IsWithinReach(target.Tile))
        {
            Send(new Info { Text = "ต้องเข้าไปใกล้ ๆ ก่อนถึงจะสร้างได้" }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (!TryGetBuildSlots(msg.EntityId, out BlueprintRequirements.Slot[] buildSlots, out string slotReason)
            || !AreBuildSlotsComplete(buildSlots, _world.GetArtifactMaterials(msg.EntityId)))
        {
            string message = slotReason ?? "ใส่วัตถุดิบสำหรับก่อสร้างยังไม่ครบ";
            Console.WriteLine("[build] ปฏิเสธ {0}: {1}", Name, message);
            Send(new Info { Text = message }, header.Seq);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        IModEventContext? beforeComplete = PluginManager.Instance?.FireEvent("building.before_complete", this, true, false,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["entity_id"] = msg.EntityId ?? "" });
        if (beforeComplete?.IsCancelled == true)
        { Send(new Info { Text = beforeComplete.CancelReason ?? "การก่อสร้างถูกยกเลิกโดยม็อด" }, header.Seq); Send(Aborts.Reason(), header.Seq); return; }
        // กันยิงซ้ำระหว่างที่ตัวเดิมยังนับเวลา 2 วิอยู่ (สถานะยังไม่เปลี่ยนจนกว่าจะครบเวลา)
        if (!_buildingNow.Add(msg.EntityId))
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: {1} กำลังสร้างอยู่แล้ว", Name, msg.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        if (_deferred.Count >= MaxPendingActions)
        {
            Console.WriteLine("[build] ปฏิเสธ {0}: มีงานค้างอยู่ {1} รายการแล้ว", Name, _deferred.Count);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        // ลงมือสร้าง = 1 หน่วยตายตัว (constants.json → build/building/energy)
        // [TodoList/04] เวลา/สตามินาสร้างตาม blueprint ของเกม (effort · energy) — ไม่ระบุใช้ effort_standard.build(level)
        // ปิดสวิตช์ Crafting.EffortFormula = 2 วิ + BuildEnergy เดิม
        float buildSeconds = 2f;
        float buildEnergy = ServerConfig.Current.Survival.BuildEnergy;
        CraftingConfig buildCfg = ServerConfig.Current.Crafting;
        if (buildCfg != null && buildCfg.EffortFormula)
        {
            _world.TryGetArtifactBlueprint(msg.EntityId, out string bpId);
            BlueprintEffortData.TryGet(bpId, out BlueprintEffortData.Info bp);
            buildSeconds = bp.Effort > 0f ? bp.Effort : buildCfg.BuildSeconds(Math.Max(1, bp.MinLevel));
            // energy ของ blueprint ใหญ่ถึง 200 (สตามินาเรามี ~100) — เกมสร้างเป็นหลายรอบ ของเราครั้งเดียวจบ จึงตัดที่ 80% ของหลอด
            if (bp.Energy > 0f) { buildEnergy = Math.Min(bp.Energy, StaminaMax * 0.8f); }
        }
        if (!TrySpendStamina(buildEnergy, ActionKind.Build))
        {
            _buildingNow.Remove(msg.EntityId);     // ไม่งั้นค้างว่า "กำลังสร้าง" ตลอดกาล
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        Console.WriteLine("[build] build {0} tile={1},{2} ใช้เวลา {3:0.#} วิ สตามินา {4:0.#}", msg.EntityId, msg.Tile.x, msg.Tile.y, buildSeconds, buildEnergy);
        Send(new Messages.Timer { Duration = buildSeconds }, header.Seq);
        _deferred.Add((Times.UnixTimeNow() + buildSeconds + 0.1, () =>
        {
            _buildingNow.Remove(msg.EntityId);
            if (!_world.TryGetArtifact(msg.EntityId, out AppearArtifact current)
                || current.States.BuildingState != BuildingState.Occupied
                || !TryGetBuildSlots(msg.EntityId, out BlueprintRequirements.Slot[] completedSlots, out _)
                || !AreBuildSlotsComplete(completedSlots, _world.GetArtifactMaterials(msg.EntityId)))
            {
                return;
            }
            // Keep the validated reservation as a construction ledger for deterministic demolition refunds.
            _world.SetArtifactBuildingState(msg.EntityId, BuildingState.Built);
            _world.BroadcastToViewers(msg.EntityId, new ArtifactBuilt { EntityId = msg.EntityId, BuilderId = EntityId });
            _world.BroadcastToViewers(msg.EntityId, new ArtifactCompleted { EntityId = msg.EntityId });
            PluginManager.Instance?.FireEvent("building.completed", this, false, true);
            _world.TryGetArtifactBlueprint(msg.EntityId, out string builtBlueprint);
            // [4 ก.ย. 2026] ไซต์ถูกส่งด้วยโมเดลเปล่า — พอสร้างเสร็จค่อยเติมโมเดลจริงแล้ว re-announce
            // (ไม่งั้นสร้างเสร็จแล้วสิ่งปลูกสร้างมองไม่เห็น) ดู ArtifactFactory.Make/BuildParts
            _world.RefreshArtifactDisplayParts(msg.EntityId, builtBlueprint);
            GainExpForBuild(builtBlueprint);
        }));
    }

    private void HandleGetArtifact(GetArtifact msg, PacketHeader header)
    {
        if (!_world.TryGetArtifact(msg.EntityId, out AppearArtifact artifact)
            || !CanModifyArtifact(artifact)
            || !IsWithinReach(artifact.Tile))
        {
            Console.WriteLine("[build] GetArtifact ปฏิเสธ {0}: ไม่มีสิทธิ์หรือไกลเกินเอื้อม {1}", Name, msg.EntityId);
            Send(Aborts.Reason(), header.Seq);
            return;
        }
        var deposited = _world.GetArtifactMaterials(msg.EntityId);
        var materials = new Dictionary<string, Item[]>();
        if (deposited != null)
        {
            foreach (var kv in deposited)
            {
                materials[kv.Key] = kv.Value.ToArray();
            }
        }
        Send(new ArtifactMaterials
        {
            EntityId = msg.EntityId,
            Materials = materials
        }, header.Seq);
    }

    private void HandleDestructArtifact(DestructArtifact msg, PacketHeader header)
    {
        Console.WriteLine("[build] destruct {0} tile={1},{2}", msg.EntityId, msg.Tile.x, msg.Tile.y);

        // GP-04: เดิม broadcast ทิ้งเลยโดยไม่ตรวจอะไร → ส่ง entityId อะไรมาก็ทุบได้ รวมถึงบ้านคนอื่น
        if (!_world.TryGetArtifact(msg.EntityId, out AppearArtifact artifact))
        {
            Console.WriteLine("[build] destruct ปฏิเสธ: ไม่รู้จัก entity '{0}'", msg.EntityId);
            Send(new Info { Text = "ไม่พบสิ่งก่อสร้างนี้" });
            Send(Aborts.Reason("ไม่พบสิ่งก่อสร้างนี้"), header.Seq);
            return;
        }
        if (!CanModifyArtifact(artifact) && !IsAdmin)
        {
            string why = string.IsNullOrEmpty(artifact.FounderEntityId)
                ? "สิ่งนี้เป็นของโลก ทุบไม่ได้ — ทุบได้เฉพาะที่ตัวเองสร้าง"
                : "ต้องเป็นเจ้าของถึงจะทุบได้";
            Console.WriteLine("[build] destruct ปฏิเสธ: {0} ไม่ใช่เจ้าของ {1}", EntityId, msg.EntityId);
            Send(new Info { Text = why });
            Send(Aborts.Reason(why), header.Seq);
            return;
        }
        // 🐛 เดิมไม่เช็คระยะ — ยิง packet ทุบของตัวเองจากอีกมุมเกาะได้
        //    (ตอนจองที่เช็ค IsWithinReach อยู่แล้ว ตอนทุบกลับไม่เช็ค — ไม่สมมาตร)
        //    สำคัญขึ้นเมื่อมีสถาปนิกร่วม เพราะจะทุบของกองกลางจากที่ไหนก็ได้
        if (!IsWithinReach(artifact.Tile))
        {
            Console.WriteLine("[build] destruct ปฏิเสธ {0}: {1} ไกลเกินเอื้อม", Name, msg.EntityId);
            Send(new Info { Text = "ต้องเข้าไปใกล้ ๆ ก่อนถึงจะทุบได้" });
            Send(Aborts.Reason("ต้องเข้าไปใกล้ ๆ ก่อนถึงจะทุบได้"), header.Seq);
            return;
        }

        IModEventContext? beforeDestroy = PluginManager.Instance?.FireEvent("building.before_destroy", this, true, false,
            new Dictionary<string, string>(StringComparer.Ordinal) { ["entity_id"] = msg.EntityId ?? "" });
        if (beforeDestroy?.IsCancelled == true)
        { Send(new Info { Text = beforeDestroy.CancelReason ?? "การทำลายถูกยกเลิกโดยม็อด" }, header.Seq); Send(Aborts.Reason(), header.Seq); return; }

        // ทุบ = 10 + ความทนทาน/2 (constants.json → build/destruct/energy)
        // เดิมทุบฟรี ไม่เสียสตามินาเลย ทั้งที่ต้นฉบับให้ทุบแพงกว่าสร้างเสียอีก
        // ⚠️ เซิร์ฟยังไม่เก็บ "ความทนทาน" ของสิ่งปลูกสร้าง (AppearArtifact ไม่มีฟิลด์นี้)
        //    จึงใช้เฉพาะส่วนฐาน 10 ไปก่อน — ต่อ durability ได้ทันทีเมื่อมีระบบนั้น
        SurvivalConfig survivalCfg = ServerConfig.Current.Survival;
        float destructEnergy = survivalCfg.DestructEnergyBase;
        if (!TrySpendStamina(destructEnergy, ActionKind.Build))
        {
            Console.WriteLine("[survival] {0} สตามินาไม่พอสำหรับทุบ (ต้องใช้ {1:F0})", Name, destructEnergy);
            Send(new Info { Text = $"สตามินาไม่พอ — ทุบของชิ้นนี้ต้องใช้ {destructEnergy:F0} หน่วย" }, header.Seq);
            Send(Aborts.Reason("สตามินาไม่พอสำหรับทุบ"), header.Seq);
            return;
        }

        Dictionary<string, List<Item>> materials = _world.TakeArtifactMaterials(msg.EntityId);
        List<Item> stored = _world.TakeAllFromBox(msg.EntityId);
        int refunded = 0;
        lock (_inventory)
        {
            if (materials != null)
            {
                foreach (var pair in materials)
                {
                    _inventory.AddRange(pair.Value);
                    refunded += pair.Value.Count;
                }
            }
            _inventory.AddRange(stored);
        }
        if (refunded > 0 || stored.Count > 0)
        {
            Console.WriteLine("[build] destruct คืนวัสดุ {0} ชิ้นและของในกล่อง {1} ชิ้นให้ {2}", refunded, stored.Count, Name);
            Send(new Info { Text = $"ได้รับวัสดุคืน {refunded} ชิ้น และของในกล่อง {stored.Count} ชิ้น" });
        }

        // โหมด Online ตัวเกมรอคำตอบ Destructing ก่อนจะเล่นท่าทุบ
        // ถ้าไม่ตอบเป็น reply ของ request นี้ ปุ่มกดแล้วเหมือนไม่เกิดอะไร
        Send(new Destructing { Duration = 1.2f, ToolType = 0 }, header.Seq);
        _world.RemoveArtifact(msg.EntityId);
        MarkDirty();
        SendInventory();
        PluginManager.Instance?.FireEvent("building.destroyed", this, false, true);
        _world.AnnounceGone(msg.EntityId);
    }

    /// <summary>ผู้เล่นคนนี้มีสิทธิ์แก้/ทุบสิ่งปลูกสร้างนี้ไหม (เป็นผู้สร้าง หรืออยู่ในรายชื่อสถาปนิก)</summary>
    private bool CanModifyArtifact(AppearArtifact artifact)
    {
        if (artifact.FounderEntityId == EntityId)
        {
            return true;
        }
        string[] architects = artifact.ArchitectEntityIds;
        if (architects != null)
        {
            for (int i = 0; i < architects.Length; i++)
            {
                if (architects[i] == EntityId)
                {
                    return true;
                }
            }
        }

        // [4 ก.ย. 2026] บั๊ก #1 (เจ้าของสั่ง): เดิมห้ามทุบของคนอื่น "ทุกที่บนเกาะ"
        // ที่ถูกคือห้ามเฉพาะ **ในอาณาเขตของเจ้าของคนนั้น** — นอกอาณาเขตทุบได้
        // (ของที่ไม่มีเจ้าของ = ของโลก ยังทุบไม่ได้เหมือนเดิม)
        if (!string.IsNullOrEmpty(artifact.FounderEntityId))
        {
            EstateRecord? estate = _world.Estates?.FindByTile(artifact.Tile.x, artifact.Tile.y);
            bool insideOwnerLand = estate != null && estate.OwnerId == artifact.FounderEntityId;
            if (!insideOwnerLand)
            {
                return true;   // นอกอาณาเขตเจ้าของ → ใครก็รื้อได้
            }
        }
        return false;
    }
}
