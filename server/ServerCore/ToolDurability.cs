using Messages;

namespace DurangoServer.Core;

/// <summary>
/// Beta 1.0 — **ความทนทานของเครื่องมือ**
///
/// ทำไมต้องมี: ก่อนหน้านี้คราฟต์ขวานครั้งเดียวใช้ได้ตลอดชีวิต ⇒ หลังชั่วโมงแรก
/// ไม่มีเหตุผลให้กลับไปหาวัสดุอีกเลย วงจร "เก็บวัสดุ → คราฟต์ → เก็บของที่ดีกว่า"
/// (ดู docs/project/GOAL.md) ขาดตรงกลาง ขวานพังได้ = มีเหตุผลให้ออกไปหาของเรื่อย ๆ
///
/// **ข้อมูลเกมไม่มีเลขความทนทานต่อไอเทม** (`durability` ที่เจอในไฟล์เป็นของสิ่งปลูกสร้าง
/// ไม่ใช่ของถือ) จึงคิดจาก **วัสดุที่ทำ** แทน ซึ่งตรงกับที่เกมสื่ออยู่แล้ว:
/// ขวานหิน &lt; ขวานกระดูก &lt; ขวานเหล็ก
/// </summary>
public static class ToolDurability
{
    /// <summary>tag ที่ถือว่าเป็น "เครื่องมือ" (ตรงกับ ToolForPrototype ใน ServerPlayer.Gathering)</summary>
    private static readonly string[] ToolTags = { "axe", "knife", "pickaxe", "shovel", "hammer", "sickle" };

    /// <summary>ชิ้นนี้เป็นเครื่องมือที่สึกหรอได้ไหม</summary>
    public static bool IsTool(string prototype)
    {
        if (string.IsNullOrEmpty(prototype))
        {
            return false;
        }
        for (int i = 0; i < ToolTags.Length; i++)
        {
            if (ItemTagData.LevelOf(prototype, ToolTags[i]) > 0)
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>Weapons, tools and wearable armor all participate in durability.</summary>
    public static bool IsDurable(string prototype)
    {
        return IsTool(prototype)
            || EquipData.TryGetWeapon(prototype, out _)
            || EquipData.TryGetArmor(prototype, out _);
    }

    /// <summary>
    /// ระดับวัสดุ 1-3 — หิน/ไม้ = 1 · กระดูก/เขา = 2 · โลหะ = 3
    ///
    /// ⚠️ **ต้องดูจากชื่อ prototype ไม่ใช่ tag วัสดุ** — tag `stone`/`bone`/`metal` ในข้อมูลเกม
    /// เป็นระดับ 1 หมดทุกอัน (บอกแค่ "ทำจากอะไร" ไม่ได้บอกว่าดีกว่ากันแค่ไหน)
    /// และบางชิ้นก็ติด tag ไม่ตรงชื่อ (`axe_tool_bone_01` ติด tag `stone`)
    ///
    /// ชื่อครอบคลุม 109 จาก 148 ชิ้น · ที่เหลือ (ของอีเวนต์/มีดครัว) ใช้ระดับของ tag เครื่องมือแทน
    /// </summary>
    public static int TierOf(string prototype)
    {
        if (string.IsNullOrEmpty(prototype))
        {
            return 1;
        }
        string p = prototype.ToLowerInvariant();
        if (p.Contains("metal") || p.Contains("brass") || p.Contains("iron") || p.Contains("steel"))
        {
            return 3;
        }
        if (p.Contains("bone") || p.Contains("horn") || p.Contains("tusk")
            || p.Contains("leather") || p.Contains("fur"))
        {
            return 2;
        }
        if (p.Contains("stone") || p.Contains("wood") || p.Contains("flint"))
        {
            return 1;
        }
        // ชื่อไม่บอก — ใช้ระดับสูงสุดของ tag เครื่องมือที่มี (1-3)
        int best = 1;
        for (int i = 0; i < ToolTags.Length; i++)
        {
            int lv = ItemTagData.LevelOf(prototype, ToolTags[i]);
            if (lv > best)
            {
                best = lv;
            }
        }
        return best > 3 ? 3 : best;
    }

    /// <summary>ความทนทานเต็มของ prototype นี้ · คืน 0 ถ้าไม่ใช่เครื่องมือ (ของทั่วไปไม่สึก)</summary>
    public static float MaxFor(string prototype)
    {
        if (!IsDurable(prototype))
        {
            return 0f;
        }
        ToolConfig cfg = ServerConfig.Current.Tools;
        if (!cfg.Enabled)
        {
            return 0f;
        }
        return cfg.DurabilityBase + TierOf(prototype) * cfg.DurabilityPerTier;
    }

    /// <summary>
    /// หลอดความทนทานที่ client เอาไปวาด
    /// ไม่ใช่เครื่องมือ ⇒ 1/1 เต็มตลอด (เหมือนเดิมก่อนมีระบบนี้ — หลอดไม่ขึ้น)
    /// </summary>
    public static Gauge MakeGauge(float current, float max)
    {
        if (max <= 0f)
        {
            return new Gauge(1f, 0f, new[] { new GaugeNode { Time = 0.0, Value = 1f } });
        }
        if (current > max)
        {
            current = max;
        }
        if (current < 0f)
        {
            current = 0f;
        }
        return new Gauge(max, 0f, new[] { new GaugeNode { Time = 0.0, Value = current } });
    }

    /// <summary>
    /// ชิ้นนี้มีความทนทานที่สึกได้จริงไหม
    ///
    /// 🐛 **ห้ามตัดสินจาก `MaxOf(item) > 0`** — ของทั่วไปก็มีหลอด 1/1 ติดมาด้วย
    /// (client ต้องการหลอดเสมอ) เคยพลาดตรงนี้จน "ผลเบอร์รี/ปลา" ไปโผล่ในรายการเครื่องมือ
    /// ตัวตัดสินคือ **prototype** เท่านั้น
    /// </summary>
    public static bool HasDurability(in Item item)
    {
        return MaxFor(item.Prototype) > 0f;
    }

    public static RepairRequirement? RepairRequirementFor(string prototype)
    {
        if (!IsDurable(prototype))
        {
            return null;
        }
        return new RepairRequirement
        {
            TagId = EquipData.TryGetArmor(prototype, out _) ? "clothes_repair_kit" : "tool_repair_kit",
            RepairPerformance = RepairPerformanceNeeded(prototype)
        };
    }

    public static int RepairPerformanceNeeded(string prototype)
    {
        return TierOf(prototype) * 10;
    }

    public static int RepairKitPerformance(string prototype)
    {
        if (string.IsNullOrEmpty(prototype)) return 0;
        if (!prototype.StartsWith("tool_repair_kit_") && !prototype.StartsWith("clothes_repair_kit_")) return 0;
        if (prototype.EndsWith("_03")) return 30;
        if (prototype.EndsWith("_02")) return 20;
        return prototype.EndsWith("_01") ? 10 : 0;
    }

    public static bool IsRepairKitFor(string targetPrototype, string kitPrototype)
    {
        RepairRequirement? requirement = RepairRequirementFor(targetPrototype);
        return requirement.HasValue
            && ItemTagData.LevelOf(kitPrototype, requirement.Value.TagId) > 0
            && RepairKitPerformance(kitPrototype) > 0;
    }

    /// <summary>ความทนทานที่เหลือของไอเทมชิ้นนี้ (อ่านจากหลอด) · 0 ถ้าไม่มีหลอด</summary>
    public static float RemainingOf(in Item item)
    {
        return item.Durability == null ? 0f : item.Durability.Get(0.0);
    }

    /// <summary>ความทนทานเต็มของไอเทมชิ้นนี้ (อ่านจากหลอด) — ดูคำเตือนที่ HasDurability</summary>
    public static float MaxOf(in Item item)
    {
        return item.Durability == null ? 0f : item.Durability.RealMax();
    }
}
