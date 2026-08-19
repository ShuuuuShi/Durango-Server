using System;
using System.Collections.Generic;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// tag ของ "โต๊ะ/เตา" (สิ่งปลูกสร้างที่ใช้คราฟต์ได้)
///
/// สูตรคราฟต์ในข้อมูลเกมระบุ <c>workbench_tags</c> เช่น <c>{"cook": 40}</c> = ต้องยืนใกล้
/// สิ่งปลูกสร้างที่ติด tag <c>cook</c> ระดับ 40 ขึ้นไป — แต่ **ตารางว่าสิ่งปลูกสร้างไหนติด tag อะไร
/// ไม่ได้ติดมากับ client** (อยู่ฝั่ง server ของ NEXON เหมือนกรณี tag เครื่องมือใน <see cref="ItemTagData"/>)
///
/// ไฟล์นี้จึงเป็นตารางที่ **เขียนเอง** จากหลักฐาน 3 อย่าง:
///   1. รายชื่อโต๊ะทั้ง 67 อันจาก TextAsset `performance` หัวข้อ <c>workbench</c>
///   2. ชื่อเกาหลีของแต่ละอันใน `blueprints` (모닥불 = กองไฟ, 큰 모닥불 = กองไฟใหญ่, 아궁이 = เตาดิน,
///      부뚜막/부엌 = ครัว, 가마 = เตาเผา, 베틀 = กี่ทอผ้า, 건조대 = ราวตาก, 작업대 = โต๊ะช่าง)
///   3. ระดับที่สูตรจริงเรียกใช้ (cook ใช้ 1/10/15/19/30/40/45/60 · table ใช้ 1..60 · kiln 1/35/55)
///
/// ⚠️ ตัวเลขคือ **ระดับสูงสุดที่โต๊ะตัวนั้นรองรับ** — ไล่จากของง่ายไปของยาก
/// กองไฟธรรมดา (cook 15) ทำอาหารพื้นฐานได้ · ต้องมีกองไฟใหญ่ (cook 40) ถึงจะต้มน้ำซุปได้
/// นี่คือจุดที่ทำให้ "เมื่อวานทำไม่ได้ วันนี้ทำได้" มีจริงในสายทำอาหาร
/// </summary>
public static class WorkbenchTagData
{
    private static Tag[] T(params (string id, int level)[] tags)
    {
        Tag[] result = new Tag[tags.Length];
        for (int i = 0; i < tags.Length; i++)
        {
            result[i] = new Tag { Id = tags[i].id, Level = tags[i].level };
        }
        return result;
    }

    /// <summary>blueprint id → tag ที่โต๊ะตัวนั้นให้</summary>
    public static readonly Dictionary<string, Tag[]> Map = new Dictionary<string, Tag[]>(StringComparer.Ordinal)
    {
        // ── ไฟ/ครัว (cook) ────────────────────────────────────────────────
        // ไล่ระดับ: กองไฟ → กองไฟใหญ่ → เตาดิน → เตา/ครัว
        { "bonfire",            T(("cook", 15)) },                       // 모닥불 กองไฟ
        { "camp_square_fire",   T(("cook", 15)) },                       // 모닥불 (แคมป์)
        { "bonfire_01",         T(("cook", 40)) },                       // 큰 모닥불 กองไฟใหญ่
        { "s02_bonfire",        T(("cook", 40)) },                       // 요리용 모닥불
        { "camping_grill",      T(("cook", 40)) },                       // 캠핑 그릴
        { "furnace_small",      T(("cook", 40)) },
        { "furnace_01",         T(("cook", 45), ("kiln", 35)) },         // 아궁이 เตาดิน (ทำอาหาร+เผาได้)
        { "camp_furnace_01",    T(("cook", 45), ("kiln", 35)) },
        { "kitchen_01",         T(("cook", 60), ("kitchen", 15)) },      // 부뚜막
        { "camp_kitchen_01",    T(("cook", 60), ("kitchen", 15)) },      // 주방
        { "kitchen_02",         T(("cook", 60), ("kitchen", 40)) },      // 부엌
        { "kitchen_03",         T(("cook", 60), ("kitchen", 60), ("urban_kitchen", 1)) },   // 도시섬 부엌
        { "kitchen_04",         T(("cook", 60), ("kitchen", 60), ("kitchen_lava", 50)) },   // 용암 부엌
        { "kitchen_table_05",   T(("cook", 60), ("table", 40)) },        // 잿나무 테이블
        { "coffee_dutch_01",    T(("cook_filter", 1)) },                 // 더치커피 기구

        // ── เตาเผา (kiln) ─────────────────────────────────────────────────
        { "kiln_01",            T(("kiln", 35)) },                       // 기본 가마
        { "camp_kiln_01",       T(("kiln", 35)) },
        { "kiln_02",            T(("kiln", 55)) },                       // 상급 가마
        { "camp_kiln_02",       T(("kiln", 55)) },
        { "kiln_04",            T(("kiln", 55)) },                       // 용암 가마

        // ── ราวตาก (dryer) ────────────────────────────────────────────────
        { "dryingrack_01",      T(("dryer", 15)) },                      // 작은 건조대
        { "camp_dryingrack_01", T(("dryer", 15)) },
        { "hide_drying",        T(("dryer", 15)) },
        { "dryingrack_02",      T(("dryer", 40)) },                      // 큰 건조대
        { "camp_dryingrack_02", T(("dryer", 40)) },
        { "dryingrack_02_t2",   T(("dryer", 40)) },
        { "clotheshorse_warp_01", T(("dryer", 40)) },
        { "clotheshorse_warp_02", T(("dryer", 40)) },

        // ── กี่ทอผ้า (loom) ───────────────────────────────────────────────
        { "loom",               T(("loom", 15)) },
        { "loom_01",            T(("loom", 15)) },                       // 작은 베틀
        { "camp_loom_01",       T(("loom", 15)) },
        { "loom_02",            T(("loom", 40)) },                       // 큰 베틀
        { "camp_loom_02",       T(("loom", 40)) },

        // ── โต๊ะช่าง (table) ───────────────────────────────────────────────
        { "fur_table",          T(("table", 15)) },                      // 간이 작업대
        { "fur_table_01",       T(("table", 25)) },                      // 기초 작업대
        { "camp_fur_table_01",  T(("table", 25)) },                      // 일반 작업대
        { "fur_table_02",       T(("table", 45)) },                      // 수련 작업대
        { "s02_fur_table",      T(("table", 45)) },
        { "fur_table_03",       T(("table", 60)) },                      // 기술 작업대
        { "fur_table_01_marble_01", T(("table", 45)) },                  // 대리석 테이블
        { "worktable_05",       T(("table", 60), ("urban_worktable", 1)) },   // 도시섬 작업대
        { "worktable_warp_01",  T(("table", 20)) },
        { "worktable_warp_02",  T(("table", 20)) },
        { "worktable_warp_03",  T(("table", 20)) },
        { "worktable_warp_04",  T(("table", 20)) },
        { "worktable_warp_05",  T(("table", 20)) },
        { "treadmill_01",       T(("table", 20)) },

        // ── โต๊ะเฉพาะทาง ──────────────────────────────────────────────────
        { "closet_table_01",    T(("table", 45), ("table_clothes", 50)) },    // 의상 작업대
        { "weapon_table_01",    T(("table", 45), ("table_weapon", 50)) },     // 무기 작업대
        { "medicine_table_01",  T(("table", 40), ("table_medicine", 40)) },   // 약 제조대
        { "medicine_table_01_t2", T(("table", 60), ("table_medicine", 60)) }, // 약 제조대 II
        { "fur_table_jewel",    T(("table", 45), ("table_jewelry", 50)) },    // 보석 세공대

        // ── สายย้อม/ปุ๋ย/หมัก ─────────────────────────────────────────────
        { "dye_01",             T(("dye_work_table", 1), ("dye_medicine_lab", 1)) },      // 염색약 제작대
        { "dye_02",             T(("dye_work_table", 1), ("dye_medicine_lab", 45)) },     // 고급 염색약 제작대
        { "dye_03",             T(("dye_work_table", 1), ("dye_medicine_lab", 45), ("urban_dye_medicine_lab", 1)) },
        { "dye_rack_01",        T(("dye_work_table", 1)) },              // 염색대
        { "fertilizer_maker_01", T(("fertilizer_maker", 1)) },           // 비료 숙성대
        { "fertilizer_maker_02", T(("fertilizer_maker", 1)) },
        { "fertilizer_maker_03", T(("fertilizer_maker", 1)) },
        { "barrel_oak_01",      T(("alcohol_ripen", 1)) },               // 오크통

        // ── หุ่นโชว์เสื้อ (ไม่มีสูตรไหนขอ แต่เกมนับเป็น workbench) ─────────────
        { "mannequin_female_01", null },
        { "mannequin_female_02", null },
        { "mannequin_female_03", null },
        { "mannequin_male_01",   null },
        { "mannequin_male_02",   null },
        { "mannequin_male_03",   null },

        // ── โต๊ะสารพัดประโยชน์ของเกม (만능작업대(치트용)) ────────────────────
        // มีไว้เทส: ติดทุก tag ระดับสูงสุด จะได้ไม่ต้องสร้างโต๊ะ 10 อันตอนเช็คสูตร
        { "allround", T(
            ("cook", 60), ("kitchen", 60), ("kitchen_lava", 60), ("cook_filter", 60),
            ("kiln", 60), ("dryer", 60), ("loom", 60), ("table", 60),
            ("table_clothes", 60), ("table_weapon", 60), ("table_medicine", 60), ("table_jewelry", 60),
            ("dye_work_table", 60), ("dye_medicine_lab", 60), ("fertilizer_maker", 60),
            ("alcohol_ripen", 60), ("urban_kitchen", 60), ("urban_worktable", 60),
            ("urban_dye_medicine_lab", 60)) }
    };

    /// <summary>โต๊ะตัวนี้ให้ tag อะไรบ้าง (ไม่ใช่โต๊ะ = null)</summary>
    public static Tag[] For(string blueprintId)
    {
        if (string.IsNullOrEmpty(blueprintId))
        {
            return null;
        }
        return Map.TryGetValue(blueprintId, out Tag[] tags) ? tags : null;
    }

    /// <summary>โต๊ะตัวนี้ติด tag นี้ระดับเท่าไร (0 = ไม่ติด)</summary>
    public static int LevelOf(string blueprintId, string tagId)
    {
        Tag[] tags = For(blueprintId);
        if (tags == null || string.IsNullOrEmpty(tagId))
        {
            return 0;
        }
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i].Id == tagId)
            {
                return tags[i].Level;
            }
        }
        return 0;
    }

    /// <summary>สิ่งปลูกสร้างชนิดนี้เป็นโต๊ะคราฟต์ไหม</summary>
    public static bool IsWorkbench(string blueprintId)
    {
        return For(blueprintId) != null;
    }

    /// <summary>
    /// เช็คว่า tag ที่สูตรเรียกใช้ มีโต๊ะสักตัวที่ให้ระดับนั้นได้จริงไหม
    /// (ไม่งั้นจะมีสูตรที่ **คราฟต์ไม่ได้เลยตลอดกาล** โดยไม่มีใครรู้)
    /// คืนรายการ "tag ระดับ N ที่ไม่มีโต๊ะไหนให้ได้" — ว่าง = ครบ
    /// </summary>
    public static List<string> FindUnreachableRequirements()
    {
        var missing = new List<string>();
        var best = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, Tag[]> pair in Map)
        {
            if (pair.Value == null)
            {
                continue;
            }
            for (int i = 0; i < pair.Value.Length; i++)
            {
                Tag t = pair.Value[i];
                if (!best.TryGetValue(t.Id, out int have) || t.Level > have)
                {
                    best[t.Id] = t.Level;
                }
            }
        }
        foreach (KeyValuePair<string, RecipeMeta.Info> pair in RecipeMeta.Map)
        {
            RecipeMeta.Tag[] need = pair.Value.Workbench;
            if (need == null || need.Length == 0)
            {
                continue;
            }
            bool ok = false;
            for (int i = 0; i < need.Length; i++)
            {
                if (best.TryGetValue(need[i].Id, out int have) && have >= need[i].Level)
                {
                    ok = true;
                    break;
                }
            }
            if (!ok)
            {
                string want = string.Empty;
                for (int i = 0; i < need.Length; i++)
                {
                    want += (i > 0 ? "/" : string.Empty) + need[i].Id + " " + need[i].Level;
                }
                missing.Add(pair.Key + " ต้องการ " + want);
            }
        }
        return missing;
    }
}
