using Shared.Ability;
using Shared.Skill;

namespace DurangoServer.Core;

/// <summary>
/// Beta 1.0 — **ค่าสถานะพื้นฐาน 8 ตัวของตัวละคร**
///
/// 🐛 เดิม `SendStatistics()` ส่งค่าคงที่ **20 เท่ากันหมดทุกคนตลอดชีพ** ⇒ หน้า "능력치" (ความสามารถ)
/// ที่เปิดให้เข้าอยู่ตลอดจึงเป็นของปลอมล้วน · เลเวล 1 กับ 20 เห็นเลขชุดเดียวกันเป๊ะ
///
/// **ค่าสถานะจริงของ NEXON กู้ไม่ได้** — ตัวเลข ability ของผู้เล่นคิดฝั่ง server ของเขา
/// ข้อมูลที่ติดมากับ client มีแค่ *ชื่อ* ของ ability กับสูตรของ **ไอเทม** เท่านั้น
/// จึงออกแบบเอง โดยยึดหลักที่ตรวจสอบได้จากตัวเกม: **ค่าสถานะโตจากสิ่งที่ตัวละครทำจริง**
///
/// <code>
/// ค่าสถานะ = Base + ((เลเวลผู้เล่น - 1) × PerLevel) + (ผลรวมความชำนาญที่โตเกินเลเวลเริ่มต้น × PerProficiency)
/// </code>
///
/// "ความชำนาญ" คือเลเวลหมวดสกิลที่ขึ้นเองจากการทำงานซ้ำ ๆ (ดู ServerPlayer.Proficiency)
/// ⇒ คนที่ล่าสัตว์ทั้งวันได้พลัง · คนที่ตีเหล็กทั้งวันได้ความคล่องมือ · ไม่ต้องมีระบบแจกแต้มใหม่
/// (เกมต้นฉบับก็ไม่มีการแจกแต้ม ability ให้ผู้เล่นกดเอง)
///
/// ทุกค่าปรับได้ที่ `data/config.json` → `abilities`
/// </summary>
public static class AbilityData
{
    /// <summary>หมวดความชำนาญที่ป้อนค่าสถานะแต่ละตัว</summary>
    public sealed class Source
    {
        public readonly Basic Ability;
        /// <summary>ชื่อไทยไว้พิมพ์ตอนดูด้วยคำสั่ง cheat</summary>
        public readonly string ThaiName;
        public readonly Category[] Categories;

        public Source(Basic ability, string thaiName, params Category[] categories)
        {
            Ability = ability;
            ThaiName = thaiName;
            Categories = categories;
        }
    }

    /// <summary>
    /// ค่าสถานะไหนโตจากงานอะไร — เลือกจากความหมายของ ability ในเกมต้นฉบับ
    ///
    /// | ค่าสถานะ | โตจาก | เหตุผล |
    /// |---|---|---|
    /// | พลัง (Strength) | ต่อสู้ประชิด · ก่อสร้าง | ฟันและแบกของหนัก |
    /// | ความอดทน (Endurance) | ป้องกัน · เอาชีวิตรอด | โดนตีแล้วยังยืนอยู่ |
    /// | ความคล่องแคล่ว (Agility) | ต่อสู้ประชิด · ต่อสู้ระยะไกล | ขยับตัวในการต่อสู้ |
    /// | ความคล่องมือ (Dexterity) | ทำอาวุธ · ทำเครื่องแต่งกาย · แปรรูป | งานฝีมือ |
    /// | การรับรู้ (Perception) | เก็บของ · ต่อสู้ระยะไกล | หาของเจอ/เล็งแม่น |
    /// | สติปัญญา (Intelligence) | ทำอาหาร · แปรรูป | รู้ว่าอะไรผสมกับอะไรได้ |
    /// | ความมุ่งมั่น (Will) | เอาชีวิตรอด · ชำแหละ | งานที่ต้องอดทนทำซ้ำ |
    /// | เสน่ห์ (Charisma) | ทำอาหาร · ทำเครื่องแต่งกาย | เลี้ยงคนเป็นและแต่งตัวเป็น |
    ///
    /// (เสน่ห์ยังไม่มีผลกับอะไรจนกว่าจะมีระบบ NPC/ตลาด — ใส่ไว้ให้หน้าตัวละครไม่มีช่องตาย)
    /// </summary>
    public static readonly Source[] Sources =
    {
        new Source(Basic.Strength, "พลัง", Category.MeleeCombat, Category.Constructing),
        new Source(Basic.Endurance, "ความอดทน", Category.Defense, Category.Survival),
        new Source(Basic.Agility, "ความคล่องแคล่ว", Category.MeleeCombat, Category.RangedCombat),
        new Source(Basic.Dexterity, "ความคล่องมือ", Category.Weaponcrafting, Category.Armorcrafting, Category.Process),
        new Source(Basic.Perception, "การรับรู้", Category.Gathering, Category.RangedCombat),
        new Source(Basic.Intelligence, "สติปัญญา", Category.Cooking, Category.Process),
        new Source(Basic.Will, "ความมุ่งมั่น", Category.Survival, Category.Butchery),
        new Source(Basic.Charisma, "เสน่ห์", Category.Cooking, Category.Armorcrafting)
    };

    /// <summary>หา source ของ ability ตัวหนึ่ง (คืน null ถ้าไม่มีในตาราง)</summary>
    public static Source Find(Basic ability)
    {
        for (int i = 0; i < Sources.Length; i++)
        {
            if (Sources[i].Ability == ability)
            {
                return Sources[i];
            }
        }
        return null;
    }
}
