using System;
using Shared.Skill;

namespace DurangoServer.Core;

/// <summary>
/// Beta 1.0 — **สกิลมีผลกับเกมจริง**
///
/// เดิมสกิลเป็นแค่ตัวเลขในไฟล์เซฟ: เรียนได้ ลืมได้ UI โชว์ครบ แต่ไม่มีผลกับอะไรเลย
///
/// วิธีคิด: รวมเลเวลสกิล**ทั้งหมดในหมวดเดียวกัน** แล้วเทียบกับ `skills.FullAt` (ค่าเริ่มต้น 60)
/// ได้ออกมาเป็นสัดส่วน 0-1 แล้วเอาไปคูณกับเพดานโบนัสของหมวดนั้น
/// (ไม่ผูกกับสกิลรายตัว เพราะข้อมูลผลของสกิลแต่ละอันอยู่ฝั่ง server ของ NEXON ไม่ได้ติดมากับ client
///  — ที่ได้มามีแค่ชื่อสกิล 275 อันกับหมวดของมัน)
///
/// หมวดที่ใช้จริงตอนนี้:
/// | หมวด | มีผลกับ |
/// |---|---|
/// | `Gathering` | เก็บของเร็วขึ้น + โอกาสได้ของเพิ่ม 1 ชิ้น |
/// | `Butchery` | แล่ซากเร็วขึ้น + โอกาสได้ชิ้นส่วนเพิ่ม |
/// | `MeleeCombat` | ดาเมจที่ตีออก |
/// | `Defense` | ดาเมจที่รับลดลง |
/// | `Weaponcrafting` `Armorcrafting` `Constructing` `Cooking` `Process` | คราฟต์เร็วขึ้น |
/// | `Survival` | ประหยัดสตามินาทุกอย่าง |
///
/// เพดานโบนัสทุกตัวปรับได้ที่ `data/config.json` → `skills`
/// </summary>
public partial class ServerPlayer
{
    private static SkillConfig SkillRates => ServerConfig.Current.Skills;

    private static readonly Random _skillRng = new Random();

    /// <summary>รวมเลเวลสกิลทุกอันในหมวดนี้ที่เรียนไว้แล้ว</summary>
    public int SkillLevelIn(Category category)
    {
        int total = 0;
        for (int i = 0; i < _knownSkills.Count; i++)
        {
            if (_knownSkills[i].Category != category || _knownSkills[i].Levels == null)
            {
                continue;
            }
            foreach (int lv in _knownSkills[i].Levels.Values)
            {
                total += lv;
            }
        }
        return total;
    }

    /// <summary>สัดส่วนความเก่งของหมวดนี้ 0-1 (ถึง skills.FullAt = เต็ม)</summary>
    private float SkillRatio(Category category)
    {
        int full = Math.Max(1, SkillRates.FullAt);
        return Math.Min(1f, SkillLevelIn(category) / (float)full);
    }

    /// <summary>สัดส่วนของหมวดคราฟต์ทั้งหมดรวมกัน (สูตรไหนก็ได้ ใช้หมวดที่เก่งที่สุด)</summary>
    private float CraftRatio()
    {
        float best = 0f;
        Category[] cats =
        {
            Category.Weaponcrafting, Category.Armorcrafting, Category.Constructing,
            Category.Cooking, Category.Process
        };
        for (int i = 0; i < cats.Length; i++)
        {
            best = Math.Max(best, SkillRatio(cats[i]));
        }
        return best;
    }

    // ── ตัวคูณที่เอาไปใช้จริงตามจุดต่าง ๆ ────────────────────────────

    /// <summary>เวลาเก็บของ × ค่านี้ (สกิลเต็ม = เร็วขึ้นตาม skills.GatherSpeed)</summary>
    public float GatherDurationScale() => 1f - SkillRatio(Category.Gathering) * SkillRates.GatherSpeed;

    /// <summary>เวลาแล่ซาก × ค่านี้</summary>
    public float ButcheryDurationScale() => 1f - SkillRatio(Category.Butchery) * SkillRates.ButcherySpeed;

    /// <summary>เวลาคราฟต์ × ค่านี้</summary>
    /// <summary>เวลาคราฟต์ × ค่านี้ (ป่วย = ช้าลงตาม Sickness.CraftDurationScale)</summary>
    public float CraftDurationScale()
    {
        float scale = 1f - CraftRatio() * SkillRates.CraftSpeed;
        if (IsSick) { scale *= Math.Max(1f, SickCfg.CraftDurationScale); }
        return scale;
    }

    /// <summary>ดาเมจที่ตีออก × ค่านี้</summary>
    public float MeleeDamageScale() => 1f + SkillRatio(Category.MeleeCombat) * SkillRates.MeleeDamage;

    /// <summary>Ranged proficiency scales bow/crossbow damage independently.</summary>
    public float RangedDamageScale() => 1f + SkillRatio(Category.RangedCombat) * SkillRates.MeleeDamage;

    /// <summary>ดาเมจที่รับ × ค่านี้</summary>
    public float DamageTakenScale() => 1f - SkillRatio(Category.Defense) * SkillRates.DefenseReduce;

    /// <summary>
    /// สตามินาที่เสีย × ค่านี้ — รวมทั้งสกิลเอาชีวิตรอด (ถาวร) และบัฟ/ดีบัฟจากอาหาร (ชั่วคราว)
    /// บัฟทำให้ถูกลง ดีบัฟทำให้แพงขึ้น · กันไม่ให้ต่ำกว่า 0.1 (ทำงานฟรี/คืนสตามินาไม่ได้)
    /// </summary>
    public float StaminaCostScale()
    {
        float scale = 1f - SkillRatio(Category.Survival) * SkillRates.StaminaSave;
        scale += StatusStaminaCostDelta();
        // ป่วย = เปลืองแรงขึ้น (ค่า "ของ" ที่เสียต่อการกระทำ)
        if (IsSick) { scale *= Math.Max(1f, SickCfg.StaminaCostScale); }
        return Math.Max(0.1f, scale);
    }

    /// <summary>เก็บของรอบนี้ได้ของเพิ่มอีกชิ้นไหม (สุ่มตามความเก่งของหมวด)</summary>
    public bool RollGatherBonus() => Roll(SkillRatio(Category.Gathering) * SkillRates.GatherBonus);

    /// <summary>แล่รอบนี้ได้ชิ้นส่วนเพิ่มไหม</summary>
    public bool RollButcheryBonus() => Roll(SkillRatio(Category.Butchery) * SkillRates.ButcheryBonus);

    /// <summary>เก็บของรอบนี้เป็น "สำเร็จมาก" ไหม — ขึ้นกับความชำนาญหมวดเก็บของ</summary>
    public bool RollGatherGreatSuccess() => Roll(0.05f + SkillRatio(Category.Gathering) * 0.20f);

    /// <summary>แล่รอบนี้สำเร็จมากไหม</summary>
    public bool RollButcheryGreatSuccess() => Roll(0.05f + SkillRatio(Category.Butchery) * 0.20f);

    /// <summary>
    /// เลเวลของไอเทมที่ได้จากงานนี้ — ฐานจากเกาะ/ทรัพยากร แล้วบวกความชำนาญ + สกิลที่เรียน
    /// สำเร็จมาก = +1 ให้เห็นผลสกิลชัดบนของที่เก็บได้
    /// </summary>
    public int ResolveSkillItemLevel(Category category, int resourceLevel, bool greatSuccess)
    {
        int resource = Math.Max(1, resourceLevel);
        int prof = Math.Max(1, ProficiencyLevel(category));
        int trained = SkillLevelIn(category);
        int level = Math.Max(resource, prof);
        level += trained / 4;
        if (greatSuccess)
        {
            level += 1;
        }
        if (level > 60)
        {
            level = 60;
        }
        return level;
    }

    private static bool Roll(float chance)
    {
        if (chance <= 0f)
        {
            return false;
        }
        lock (_skillRng)
        {
            return _skillRng.NextDouble() < chance;
        }
    }

    /// <summary>สรุปโบนัสของตัวเองไว้ตอบคำสั่ง `cheat skills` (ดูว่าที่เรียนไปมีผลจริงไหม)</summary>
    public string DescribeSkillBonuses()
    {
        return string.Format(
            "เก็บของ {0} (เร็วขึ้น {1:P0} · โบนัส {2:P0}) · แล่เนื้อ {3} (เร็วขึ้น {4:P0}) · " +
            "ต่อสู้ {5} (ดาเมจ +{6:P0}) · ป้องกัน {7} (รับ -{8:P0}) · คราฟต์ (เร็วขึ้น {9:P0}) · " +
            "เอาชีวิตรอด {10} (สตามินา -{11:P0})",
            SkillLevelIn(Category.Gathering), 1f - GatherDurationScale(), SkillRatio(Category.Gathering) * SkillRates.GatherBonus,
            SkillLevelIn(Category.Butchery), 1f - ButcheryDurationScale(),
            SkillLevelIn(Category.MeleeCombat), MeleeDamageScale() - 1f,
            SkillLevelIn(Category.Defense), 1f - DamageTakenScale(),
            1f - CraftDurationScale(),
            SkillLevelIn(Category.Survival), 1f - StaminaCostScale());
    }
}
