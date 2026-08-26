using System.Collections.Generic;
using Messages;
using Shared.Building;
using Shared.Etc;
using Shared.Skill;

namespace DurangoServer.Core;

/// <summary>
/// GP-07: รูปแบบข้อมูลที่เขียนลงดิสก์
///
/// ตั้งใจเก็บเป็น "record ของเราเอง" ไม่ใช่ serialize struct ของ Messages ตรง ๆ เพราะ
/// 1) <c>Item.Ext</c> เป็น <c>object</c> — Newtonsoft deserialize กลับมาได้เป็น JObject ไม่ใช่ชนิดเดิม
///    ต้องพึ่ง TypeNameHandling ซึ่งทำให้ไฟล์อ่านยากและผูกกับชื่อ assembly
/// 2) struct ของ Messages ต้องตรงกับ client เป๊ะ ๆ ห้ามแตะ — ถ้าผูกไฟล์เซฟไว้กับมัน
///    วันหลังอัปเดต client แล้วเซฟเก่าจะพังทันที
///
/// ⚠️ ข้อแลก: ฟิลด์ที่ไม่ได้อยู่ในนี้จะไม่ถูกเก็บ ตอนนี้ครอบคลุมของที่มีจริงครบแล้ว
/// (ไอเทมที่คราฟต์/เก็บได้ยังไม่มี Tags/Performance) ถ้าวันหลังไอเทมมีข้อมูลมากขึ้นต้องมาเพิ่มที่นี่ด้วย
/// </summary>
public abstract class SaveEnvelope
{
    /// <summary>เวอร์ชันไฟล์เซฟ — เพิ่มเลขเมื่อโครงสร้างเปลี่ยนจนอ่านของเก่าไม่ได้</summary>
    public int Version { get; set; } = 1;
}

public sealed class ItemSave
{
    public string Id { get; set; }
    public string Prototype { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public string Icon { get; set; }
    public int Level { get; set; } = 1;
    public int Size { get; set; } = 1;
    public string GeneratorId { get; set; }

    /// <summary>ไม่ null = ไอเทมนี้เป็นแคปซูลของสิ่งปลูกสร้าง</summary>
    public string CapsuleBlueprintId { get; set; }

    /// <summary>
    /// ความทนทานที่เหลือของเครื่องมือ · **−1 = ยังไม่เคยกำหนด** (เซฟเก่าก่อนมีระบบนี้)
    /// เจอ −1 ตอนโหลด = เติมให้เต็มตามวัสดุ ไม่ใช่ปล่อยเป็น 0 แล้วขวานพังทันทีที่ล็อกอิน
    /// ไอเทมที่ไม่ใช่เครื่องมือจะเป็น −1 ตลอดไปและไม่มีผลอะไร
    /// </summary>
    public float Durability { get; set; } = -1f;

    /// <summary>
    /// ของชิ้นนี้ผ่านการแปรรูปมาแล้วไหม (ย่าง/ต้ม/ตากแห้ง — สูตร Modify)
    /// ไม่เก็บไว้ = ออกเกมแล้วเข้าใหม่ เนื้อย่างกลับไปเป็นเนื้อดิบ เพราะ tag สร้างจาก prototype ล้วน
    /// </summary>
    public bool Processed { get; set; }

    public static ItemSave From(Item item)
    {
        string capsule = null;
        if (item.Ext is ArtifactCapsule cap)
        {
            capsule = cap.BlueprintId;
        }
        return new ItemSave
        {
            Id = item.Id,
            Prototype = item.Prototype,
            Name = item.Name,
            Description = item.Description,
            Icon = item.Icon,
            Level = item.Level,
            Size = item.Size,
            GeneratorId = item.GeneratorId,
            CapsuleBlueprintId = capsule,
            // เก็บเฉพาะของที่มีความทนทานจริง ๆ ไฟล์เซฟจะได้ไม่รกด้วยเลข −1 ทุกบรรทัด
            Durability = ToolDurability.HasDurability(item) ? ToolDurability.RemainingOf(item) : -1f,
            // ดิบตามตาราง prototype แต่ tag ที่ติดมากับชิ้นนี้ไม่ดิบแล้ว = ผ่านการแปรรูปมา
            Processed = ItemTagData.LevelOf(item.Prototype, ItemProcessing.RawTag) > 0 && !ItemProcessing.IsRaw(item)
        };
    }

    public Item ToItem()
    {
        // เซฟเก่า (Durability = −1) หรือของที่เพิ่งกลายเป็นเครื่องมือเพราะเราปรับตาราง
        // ⇒ เริ่มที่เต็มหลอด · ไม่ใช่เครื่องมือ ⇒ MaxFor คืน 0 แล้ว MakeGauge ให้หลอด 1/1 เหมือนเดิม
        float max = ToolDurability.MaxFor(Prototype);
        float current = Durability >= 0f ? Durability : max;
        return new Item
        {
            Id = Id,
            Name = Name,
            Description = Description,
            Icon = Icon,
            SubIcon = null,
            Prototype = Prototype,
            Level = Level,
            OriginalLevel = Level,
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
            Size = Size,
            Durability = ToolDurability.MakeGauge(current, max),
            ColorR = "FFFFFF",
            ColorG = "FFFFFF",
            ColorB = "FFFFFF",
            Unstable = false,
            RepairRequirement = ToolDurability.RepairRequirementFor(Prototype),
            FounderId = null,
            FounderCategory = null,
            Tags = Processed ? ItemProcessing.ProcessedTags(Prototype) : ItemTagData.For(Prototype),
            TagModifications = null,
            // แนบช่องที่ใส่ได้ไปด้วย ไม่งั้น client กดใส่อุปกรณ์ไม่ได้ (ดู EquipData.PerformanceFor)
            Performance = EquipData.PerformanceFor(Prototype),
            Ext = string.IsNullOrEmpty(CapsuleBlueprintId)
                ? null
                : new ArtifactCapsule
                {
                    EntityId = null,
                    BlueprintId = CapsuleBlueprintId,
                    ArtifactLevel = 1,
                    Tags = null,
                    Performance = null,
                    Display = default,
                    State = default,
                    LookNames = null,
                    OccupySize = new Point2(1, 1)
                },
            CollectibleId = null,
            GeneratorId = GeneratorId,
            EmotionalMotions = null,
            PioneerCost = 0f
        };
    }
}

public sealed class SkillBundleSave
{
    public int Category { get; set; }
    public string SkillId { get; set; }
    public Dictionary<string, int> Levels { get; set; }

    public static SkillBundleSave From(SkillBundle b)
    {
        return new SkillBundleSave
        {
            Category = (int)b.Category,
            SkillId = b.SkillId,
            Levels = b.Levels != null ? new Dictionary<string, int>(b.Levels) : new Dictionary<string, int>()
        };
    }

    public SkillBundle ToBundle()
    {
        return new SkillBundle
        {
            Category = (Category)Category,
            SkillId = SkillId,
            Levels = Levels ?? new Dictionary<string, int>()
        };
    }
}

public sealed class PlayerSave : SaveEnvelope
{
    public string EntityId { get; set; }
    public string Name { get; set; }
    public int Level { get; set; }
    public ushort EntityType { get; set; }

    /// <summary>GP-14: หน้าตาที่ใช้ล่าสุด — ใช้ตอน login รอบหน้าถ้า client ไม่ได้ส่งมา</summary>
    public string DisplayJson { get; set; }

    // WorldPosition เป็นพิกัด 2 มิติ (x, y) — ความสูงอยู่ใน Location.Height ซึ่งยังไม่ได้ใช้
    public float PosX { get; set; }
    public float PosY { get; set; }
    public float Yaw { get; set; }
    public bool HasPosition { get; set; }

    public int SkillPoints { get; set; }

    /// <summary>Beta 1.0 — exp สะสม (เลเวลคิดใหม่จากค่านี้ตอนโหลด ดู ServerPlayer.Progress)</summary>
    public int TotalExp { get; set; }

    /// <summary>
    /// Beta 1.1 — ออกจากเกมครั้งล่าสุดที่เกาะไหน
    /// เข้ามาคนละเกาะกับที่จำไว้ = เกิดที่จุดเข้าเกมของเกาะใหม่ (ไม่ใช่พิกัดเดิมของอีกเกาะ)
    /// </summary>
    public string LastIsland { get; set; }

    /// <summary>เควส — ความคืบหน้า/ทำเสร็จแล้ว/รับรางวัลแล้ว (ดู ServerPlayer.Quests)</summary>
    public Dictionary<string, int> QuestProgress { get; set; } = new Dictionary<string, int>();
    public List<string> QuestDone { get; set; } = new List<string>();
    public List<string> QuestRewarded { get; set; } = new List<string>();
    /// <summary>เควสที่เคยเด้งข้อความ "เควสใหม่" ไปแล้ว — กันเด้งซ้ำทุก login</summary>
    public List<string> QuestAnnounced { get; set; } = new List<string>();

    public List<ItemSave> Inventory { get; set; } = new List<ItemSave>();
    public List<string> InventoryOrder { get; set; } = new List<string>();
    public List<string> LockedItemIds { get; set; } = new List<string>();
    public List<SkillBundleSave> KnownSkills { get; set; } = new List<SkillBundleSave>();

    /// <summary>ได้ของแถมตอนเข้าเกมครั้งแรกไปแล้วหรือยัง (กันแจกกองไฟซ้ำทุก login)</summary>
    public bool StarterGiven { get; set; }

    /// <summary>เฟส C — อุปกรณ์ที่ใส่อยู่: ช่อง → item id</summary>
    public Dictionary<string, string> EquippedItems { get; set; } = new Dictionary<string, string>();
    public int CurrentEquipSlotType { get; set; } = 1;
    public Dictionary<string, Dictionary<string, string>> EquipmentPresets { get; set; } =
        new Dictionary<string, Dictionary<string, string>>();
    public string AccessoryId { get; set; }
    public bool HasDeathPoint { get; set; }
    public int DeathTileX { get; set; }
    public int DeathTileY { get; set; }
    public int ImmediateReviveCount { get; set; }
    /// <summary>ตายอยู่ไหม — ต้อง persist ไม่งั้นรีสตาร์ทเซิร์ฟแล้วคนตายฟื้นเอง (auto-revive)</summary>
    public bool Dead { get; set; }

    /// <summary>เฟส C — ค่าสถานะ (เลือด/สตามินา/ความล้า)</summary>
    public SurvivalSave Survival { get; set; }

    /// <summary>
    /// ความชำนาญของหมวดสกิล: เลข enum Shared.Skill.Category -> exp รวม
    /// เก็บ exp รวมแล้วคิดเลเวลใหม่ตอนโหลด (แบบเดียวกับเลเวลผู้เล่น) ⇒ ปรับตารางแล้วเซฟเก่าไม่เพี้ยน
    /// </summary>
    public Dictionary<string, int> CategoryExp { get; set; } = new Dictionary<string, int>();
    public List<string> CompletedCategoryResearch { get; set; } = new List<string>();
    public int ResearchCategory { get; set; } = -1;
    public int ResearchTargetLevel { get; set; }
    public double ResearchStartedAt { get; set; }
    public double ResearchEndsAt { get; set; }

    // Group 2: character systems that must survive reconnects.
    public Dictionary<string, int> ResistanceExp { get; set; } = new Dictionary<string, int>();
    public List<StatusEffectSave> StatusEffects { get; set; } = new List<StatusEffectSave>();
    public string SelectedTitleId { get; set; }
    public string TargetTitleId { get; set; }

    /// <summary>
    /// รอยแยก/วาร์ปเรกเซเลอเรเตอร์ — Warp Matter สะสม (ดู ServerPlayer.WarpAccelerator.cs)
    /// ตัวนับเดี่ยว ๆ ไม่ใช่ Wallet/Currency เต็มระบบ (เซิร์ฟยังไม่มีระบบกระเป๋าเงินจริง)
    /// </summary>
    public int WarpMatterBalance { get; set; }

    /// <summary>Warp Matter ที่ได้ไปแล้วในสัปดาห์ปัจจุบัน (เทียบกับ WarpAcceleratorConfig.WeeklyWarpMatterCap)</summary>
    public int WeeklyWarpMatterAcquired { get; set; }

    /// <summary>เวลาที่ตัวนับรายสัปดาห์จะรีเซ็ตครั้งถัดไป (unix seconds) — 0 = ยังไม่เคยตั้ง</summary>
    public double WeeklyWarpMatterRefreshAt { get; set; }
}

public sealed class StatusEffectSave
{
    public string Id { get; set; }
    public string EffectId { get; set; }
    public int Level { get; set; }
    public double Since { get; set; }
    public double Until { get; set; }
    public bool Enabled { get; set; } = true;
}

/// <summary>เฟส C — ค่าสถานะเอาชีวิตรอด</summary>
public sealed class SurvivalSave
{
    public float Life { get; set; }
    public float Stamina { get; set; }
    public float Fatigue { get; set; }
    public float Hungry { get; set; }
    public bool HasHungry { get; set; }
}

public sealed class ArtifactSave
{
    public string EntityId { get; set; }
    public ushort EntityType { get; set; }
    public string BlueprintId { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }
    public int SizeX { get; set; } = 1;
    public int SizeY { get; set; } = 1;
    public int Rotation { get; set; }
    public int? Floor { get; set; }
    public int Stories { get; set; } = 1;
    public int BuildingState { get; set; }
    public string FounderEntityId { get; set; }
    public string[] ArchitectEntityIds { get; set; }

    public static ArtifactSave From(AppearArtifact a, string blueprintId)
    {
        return new ArtifactSave
        {
            EntityId = a.EntityId,
            EntityType = a.EntityType,
            BlueprintId = blueprintId,
            TileX = a.Tile.x,
            TileY = a.Tile.y,
            SizeX = a.Size.x,
            SizeY = a.Size.y,
            Rotation = (int)a.Rotation,
            Floor = a.Floor,
            Stories = a.Stories ?? 1,
            BuildingState = (int)a.States.BuildingState,
            FounderEntityId = a.FounderEntityId,
            ArchitectEntityIds = a.ArchitectEntityIds
        };
    }

    public AppearArtifact ToArtifact()
    {
        return ArtifactFactory.Make(
            FounderEntityId,
            EntityId,
            EntityType,
            new Point2(TileX, TileY),
            new Point2(SizeX, SizeY),
            (Rotation)Rotation,
            Floor,
            Stories,
            BlueprintId,
            (BuildingState)BuildingState,
            ArchitectEntityIds);
    }
}

public sealed class WorldSave : SaveEnvelope
{
    public string TerrainId { get; set; }
    public List<ArtifactSave> Artifacts { get; set; } = new List<ArtifactSave>();

    /// <summary>ต้นไม้/ก้อนหินที่ถูกเก็บจนหมดไปแล้ว (พิกัด tile) — เอาไปลบออกจาก Garden ตอนโหลด</summary>
    public List<int[]> RemovedNaturals { get; set; } = new List<int[]>();

    /// <summary>เฟส C — ของในกล่องเก็บของ: entity id ของกล่อง → รายการไอเทม</summary>
    public Dictionary<string, List<ItemSave>> Boxes { get; set; } = new Dictionary<string, List<ItemSave>>();

    /// <summary>แปลงผักที่ปลูกไว้ (key อยู่ใน FarmSave.ArtifactId)</summary>
    public List<FarmSave> Farms { get; set; } = new List<FarmSave>();
}

/// <summary>
/// แปลงผัก 1 แปลง
///
/// เก็บ <see cref="RemainProduct"/>/<see cref="RemainSeed"/> ด้วย เพราะถ้าเก็บแค่ "โตแล้ว"
/// พอรีสตาร์ทเซิร์ฟ ระบบจะคิดผลผลิตใหม่เต็มจำนวน = ปั๊มของด้วยการรีสตาร์ท
/// </summary>
public sealed class FarmSave
{
    public string ArtifactId { get; set; }
    public int TileX { get; set; }
    public int TileY { get; set; }
    public string SeedId { get; set; }
    public int SeedLevel { get; set; } = 1;
    public double PlantedAt { get; set; }
    public double GrowsUntil { get; set; }
    public float Water { get; set; }
    public float Fertilizer { get; set; }
    public int Fitness { get; set; }
    public bool Resolved { get; set; }
    public bool Dead { get; set; }
    public string Look { get; set; }
    public int RemainProduct { get; set; }
    public int RemainSeed { get; set; }

    public static FarmSave From(ServerWorld.FarmPlot p, int remainProduct, int remainSeed)
    {
        return new FarmSave
        {
            ArtifactId = p.ArtifactId,
            TileX = p.TileX,
            TileY = p.TileY,
            SeedId = p.SeedId,
            SeedLevel = p.SeedLevel,
            PlantedAt = p.PlantedAt,
            GrowsUntil = p.GrowsUntil,
            Water = p.Water,
            Fertilizer = p.Fertilizer,
            Fitness = (int)p.Fitness,
            Resolved = p.Resolved,
            Dead = p.Dead,
            Look = p.Look,
            RemainProduct = remainProduct,
            RemainSeed = remainSeed
        };
    }

    public ServerWorld.FarmPlot ToPlot()
    {
        return new ServerWorld.FarmPlot
        {
            ArtifactId = ArtifactId,
            TileX = TileX,
            TileY = TileY,
            SeedId = SeedId,
            SeedLevel = SeedLevel < 1 ? 1 : SeedLevel,
            PlantedAt = PlantedAt,
            GrowsUntil = GrowsUntil,
            Water = Water,
            Fertilizer = Fertilizer,
            Fitness = (Shared.Etc.Fitness)Fitness,
            Resolved = Resolved,
            Dead = Dead,
            Look = Look,
            RemainProduct = RemainProduct,
            RemainSeed = RemainSeed
        };
    }
}
