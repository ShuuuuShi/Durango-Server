using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// **สกิลไหนปลดล็อกท่าต่อสู้อะไร** — ค่าจริงจากข้อมูลเกม
///
/// สกัดอัตโนมัติด้วย scripts/extract_action_unlocks.py - **อย่าแก้ด้วยมือ**
///
/// ใช้ที่ `HandleUseBattleAction` เพื่อตรวจว่าผู้เล่นเรียนสกิลที่ปลดล็อกท่านี้แล้วจริงไหม
/// เดิมตรวจแค่ tag อาวุธ ⇒ modded client ใช้ท่าได้ทุกอย่างโดยไม่เรียนสกิล
///
/// สรุปจำนวน: ท่าทั้งหมด 59 - ปลดล็อกด้วยสกิล 32 - **ได้ตั้งแต่แรก 27**
/// </summary>
public static class ActionUnlockData
{
    /// <summary>ท่าที่ไม่มีสกิลไหนปลดล็อก = ทุกคนได้ตั้งแต่เริ่ม (ท่าพื้นฐาน/หลบ/เทคลิ)</summary>
    public static readonly string[] AlwaysActions = new[] { "barehand_default_a", "barehand_default_b", "onehand_default_a", "onehand_default_axe_a", "onehand_default_axe_b", "onehand_default_axe_c", "onehand_default_b", "onehand_default_blunt_a", "onehand_default_blunt_b", "onehand_default_blunt_c", "onehand_default_c", "ranged_bow_default_a", "ranged_bow_default_b", "ranged_bow_default_c", "ranged_crossbow_default", "twohand_default_a", "twohand_default_axe_a", "twohand_default_axe_b", "twohand_default_axe_c", "twohand_default_b", "twohand_default_blunt_a", "twohand_default_blunt_b", "twohand_default_blunt_c", "twohand_default_c", "twohand_lance_default_a", "twohand_lance_default_b", "twohand_lance_default_c" };

    /// <summary>
    /// คีย์ <c>"skillId|subId"</c> → รายการท่าต่อเลเวล (index 0 = เลเวล 1)
    /// เรียนสกิลถึงเลเวล N = ได้ท่าจาก index 0 ถึง N-1 ทั้งหมด
    /// </summary>
    public static readonly Dictionary<string, string[][]> BySkill = new Dictionary<string, string[][]>
    {
        { "aimed_shot|__base__", new[]
          {
            new[] { "ranged_bow_aimedshot", "ranged_crossbow_aimedshot" },
            new string[0],
            new string[0],
            new string[0]
          } },
        { "dodge|__base__", new[]
          {
            new[] { "barehand_dodge", "onehand_dodge", "twohand_dodge" }
          } },
        { "kick|__base__", new[]
          {
            new[] { "barehand_kick_a", "barehand_kick_b" }
          } },
        { "lance_dash|__base__", new[]
          {
            new[] { "twohand_lance_dash" },
            new string[0],
            new string[0],
            new string[0]
          } },
        { "lance_strike|__base__", new[]
          {
            new[] { "twohand_lance_strike" },
            new string[0],
            new string[0],
            new string[0]
          } },
        { "onehanded_flurry|__base__", new[]
          {
            new[] { "onehand_flurry", "onehand_flurry_axe", "onehand_flurry_blunt" },
            new string[0],
            new string[0],
            new string[0]
          } },
        { "onehanded_smash|__base__", new[]
          {
            new[] { "onehand_smash", "onehand_smash_axe", "onehand_smash_blunt" },
            new string[0],
            new string[0],
            new string[0]
          } },
        { "onehanded_stab|__base__", new[]
          {
            new[] { "onehand_stab", "onehand_stab_axe", "onehand_stab_blunt" },
            new string[0],
            new string[0],
            new string[0]
          } },
        { "quick_shot|__base__", new[]
          {
            new[] { "ranged_bow_quickshot", "ranged_crossbow_quickshot" },
            new string[0],
            new string[0],
            new string[0]
          } },
        { "reckless|__base__", new[]
          {
            new[] { "barehand_smash" },
            new[] { "barehand_combination" }
          } },
        { "tackle|__base__", new[]
          {
            new[] { "melee_tackle" }
          } },
        { "twohanded_smash|__base__", new[]
          {
            new[] { "twohand_smash", "twohand_smash_axe", "twohand_smash_blunt" },
            new string[0],
            new string[0],
            new string[0]
          } },
        { "twohanded_strike|__base__", new[]
          {
            new[] { "twohand_strike", "twohand_strike_axe", "twohand_strike_blunt" },
            new string[0],
            new string[0],
            new string[0]
          } },
        { "twohanded_sweeping|__base__", new[]
          {
            new[] { "twohand_sweeping", "twohand_sweeping_axe", "twohand_sweeping_blunt" },
            new string[0],
            new string[0],
            new string[0]
          } },
    };

    /// <summary>ท่าที่สกิลนี้ให้เมื่อเรียนถึงเลเวลที่กำหนด (สะสมตั้งแต่เลเวล 1)</summary>
    public static void Collect(string skillId, string subId, int level, HashSet<string> actions)
    {
        if (!BySkill.TryGetValue(skillId + "|" + subId, out string[][] levels))
        {
            return;
        }
        int upto = level < levels.Length ? level : levels.Length;
        for (int i = 0; i < upto; i++)
        {
            for (int j = 0; j < levels[i].Length; j++)
            {
                actions.Add(levels[i][j]);
            }
        }
    }
}
