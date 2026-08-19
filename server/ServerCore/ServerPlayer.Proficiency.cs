using System;
using System.Collections.Generic;
using Durango.Utils;
using Messages;
using Shared.Skill;

namespace DurangoServer.Core;

// ============================================================================
// ServerPlayer.Proficiency — ความชำนาญของหมวดสกิล (เลเวลที่ขึ้นเองจากการทำงาน)
//
// เกมนี้มีสกิล 2 ชั้นที่คนละเรื่องกัน:
//   1. **สกิลย่อย** — ใช้แต้มจากการขึ้นเลเวลผู้เล่นไปกดเรียนเอง (ServerPlayer.Skills)
//   2. **ความชำนาญของหมวด** — ล่า/เก็บของ/ทำอาหาร/คราฟต์/ก่อสร้าง ขึ้นเองจากการทำซ้ำ ๆ
//
// 🐛 เดิมทำแต่ชั้นที่ 1 ⇒ หน้าสกิลโชว์ทุกหมวดเป็นเลเวล 0 ตลอดกาล
//    ผู้เล่นที่เก็บของทั้งวันก็ยังเป็นมือใหม่เท่าเดิม ("สกิลอัตโนมัติไม่อัพให้เลย" — เจอตอนเล่นจริง)
//
// สเกล exp เป็นของเกมจริง: **ทำสำเร็จ 1 ครั้ง = 1 exp** (เลเวล 1→2 ใช้ 1 · 40→41 ใช้ 27)
// ดูตารางที่ SkillCategoryData.cs
// ============================================================================

public partial class ServerPlayer
{
    /// <summary>exp รวมของแต่ละหมวด (เก็บ exp รวม แล้วคิดเลเวลใหม่ทุกครั้ง — เหมือนเลเวลผู้เล่น)</summary>
    private readonly Dictionary<Category, int> _categoryExp = new Dictionary<Category, int>();

    /// <summary>
    /// เพดานเลเวลของหมวด — 0 = ใช้เพดานของตารางเอง (60)
    ///
    /// **ไม่ผูกกับเลเวลผู้เล่น** เพราะในเกมจริงเงื่อนไข "พร้อมวิจัย" คือ เลเวลหมวด &lt; เลเวลตัวละคร
    /// (Durango.Logic.Skill.Category.IsReadyToResearch) ซึ่งจะไม่มีความหมายเลยถ้าหมวดขึ้นเกินตัวละครไม่ได้
    /// ⇒ หมวดขึ้นได้อิสระตามการทำงาน · เลเวลตัวละครก็ขึ้นจากงานเดียวกันอยู่แล้ว จึงไม่หลุดกันมาก
    /// </summary>
    private int CategoryLevelCap => 0;

    /// <summary>เลเวลความชำนาญของหมวดนี้ตอนนี้</summary>
    public int ProficiencyLevel(Category category)
    {
        _categoryExp.TryGetValue(category, out int total);
        ResolveProficiency(category, total, out int level, out int _);
        return level;
    }

    /// <summary>
    /// ได้ความชำนาญจากการทำงานสำเร็จ 1 ครั้ง
    ///
    /// เรียก **ตอนทำสำเร็จจริงเท่านั้น** (แบบเดียวกับความสึกของเครื่องมือ) ไม่ใช่ตอนกดสั่ง
    /// ไม่งั้นยิง packet รัว ๆ ที่ถูกปฏิเสธก็อัพสกิลได้
    /// </summary>
    public void GainProficiency(Category category, int amount = 1)
    {
        if (category == Category.Invalid || amount <= 0)
        {
            return;
        }
        if (!ServerConfig.Current.Features.Skills)
        {
            return;
        }
        SkillConfig cfg = ServerConfig.Current.Skills;
        if (!cfg.ProficiencyEnabled)
        {
            return;
        }
        if (!SkillCategoryData.TryGet(category, out SkillCategoryData.Curve _))
        {
            return;
        }

        _categoryExp.TryGetValue(category, out int before);
        ResolveProficiency(category, before, out int levelBefore, out int _);

        int gained = (int)Math.Round(amount * cfg.ProficiencyRate);
        if (gained <= 0)
        {
            gained = amount > 0 ? 1 : 0;      // เรทต่ำแค่ไหนก็ต้องขยับได้ ไม่งั้นระบบตายเงียบ
        }
        int after = before + gained;
        _categoryExp[category] = after;
        ResolveProficiency(category, after, out int levelAfter, out int _);
        MarkDirty();

        if (levelAfter > levelBefore)
        {
            Console.WriteLine("[proficiency] {0}: {1} เลเวล {2} → {3}", Name, category, levelBefore, levelAfter);
            Send(new Info { Text = $"ความชำนาญ{ProficiencyNameOf(category)}ขึ้นเป็นเลเวล {levelAfter}" });
            SendSkills();       // หน้าสกิลต้องอัปเดตทันที ไม่ใช่รอเปิดเมนูใหม่
            // ความชำนาญเป็นตัวป้อนค่าสถานะ 8 ตัว ⇒ หลอดเลือด/สตามินาและหน้าตัวละครต้องขยับตาม
            RefreshAbilities();
        }
        else if (after % 5 == 0)
        {
            // ไม่ส่งทุกครั้งเพราะ 1 แอ็กชัน = 1 packet คูณจำนวนผู้เล่นตอนฟาร์มรัว ๆ
            SendSkills();
        }
    }

    /// <summary>ชื่อไทยของหมวด — เอาไปขึ้นข้อความตอนเลเวลขึ้น</summary>
    private static string ProficiencyNameOf(Category category)
    {
        switch (category)
        {
            case Category.Survival: return "การเอาชีวิตรอด";
            case Category.MeleeCombat: return "การต่อสู้ระยะประชิด";
            case Category.RangedCombat: return "การต่อสู้ระยะไกล";
            case Category.Defense: return "การป้องกัน";
            case Category.Butchery: return "การชำแหละ";
            case Category.Gathering: return "การเก็บของ";
            case Category.Cooking: return "การทำอาหาร";
            case Category.Weaponcrafting: return "การทำอาวุธ";
            case Category.Armorcrafting: return "การทำเครื่องแต่งกาย";
            case Category.Constructing: return "การก่อสร้าง";
            case Category.Farming: return "การเพาะปลูก";
            case Category.Process: return "การแปรรูป";
            default: return category.ToString();
        }
    }

    /// <summary>
    /// หมวดที่ได้ความชำนาญจากการคราฟต์สูตรนี้ — ดูจากหมวดของสูตรในข้อมูลเกม
    /// (ทำอาหารขึ้นหมวดทำอาหาร · ทำดาบขึ้นหมวดทำอาวุธ · ฟั่นเชือกขึ้นหมวดแปรรูป)
    /// </summary>
    private static Category CraftCategoryOf(RecipeMeta.Info meta)
    {
        if (meta == null)
        {
            return Category.Process;
        }
        switch (meta.Category)
        {
            case "cook":
            case "cook_season2":
                return Category.Cooking;
            case "weapon_and_tool":
            case "tool":
            case "tool_season2":
            case "modular_attach":
                return Category.Weaponcrafting;
            case "clothing":
            case "clothing_season2":
                return Category.Armorcrafting;
            default:
                return Category.Process;
        }
    }

    // ---- เซฟ/โหลด ----

    /// <summary>exp ของทุกหมวดสำหรับเขียนลงไฟล์เซฟ (หมวดที่ยังไม่เคยทำจะไม่ถูกเก็บ)</summary>
    private Dictionary<string, int> BuildProficiencySave()
    {
        var save = new Dictionary<string, int>();
        foreach (KeyValuePair<Category, int> pair in _categoryExp)
        {
            if (pair.Value > 0)
            {
                save[((int)pair.Key).ToString()] = pair.Value;
            }
        }
        return save;
    }

    private void ApplyProficiencySave(Dictionary<string, int> save)
    {
        _categoryExp.Clear();
        if (save == null)
        {
            return;
        }
        foreach (KeyValuePair<string, int> pair in save)
        {
            if (int.TryParse(pair.Key, out int catId) && pair.Value > 0)
            {
                _categoryExp[(Category)catId] = pair.Value;
            }
        }
    }
}
