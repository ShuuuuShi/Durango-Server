using System;
using System.Collections.Generic;
using Shared.Skill;

namespace DurangoServer.Core;

/// <summary>
/// เทียบ SkillNodeData (ตารางโหนดที่เซิร์ฟใช้) กับ SkillData (id ที่ดึงจากไฟล์สกิลของ client)
/// พิมพ์รายการที่ไม่ตรงตอนบูต — กันเคสเลเวลสกิลเกินจำนวนโหนดที่ client มี (NRE แพ็กเก็ต Skills)
/// </summary>
public static class SkillParity
{
    public static void Report()
    {
        var maxLevelBySkill = new Dictionary<string, int>(StringComparer.Ordinal);
        foreach (KeyValuePair<string, SkillNodeData.Node> pair in SkillNodeData.Map)
        {
            // key = skillId|subId|level
            string[] parts = pair.Key.Split('|');
            if (parts.Length < 3)
            {
                continue;
            }
            string skillId = parts[0];
            int level = pair.Value.Level;
            if (!maxLevelBySkill.TryGetValue(skillId, out int max) || level > max)
            {
                maxLevelBySkill[skillId] = level;
            }
        }

        int missingNodes = 0;
        int extraNodes = 0;
        foreach (KeyValuePair<string, int> pair in SkillData.SkillCategory)
        {
            if (!maxLevelBySkill.ContainsKey(pair.Key))
            {
                missingNodes++;
                if (missingNodes <= 20)
                {
                    Console.WriteLine($"[skill-parity] client มีสกิล '{pair.Key}' แต่ SkillNodeData ไม่มีโหนด");
                }
            }
        }
        foreach (string skillId in maxLevelBySkill.Keys)
        {
            if (!SkillData.SkillCategory.ContainsKey(skillId))
            {
                extraNodes++;
                if (extraNodes <= 20)
                {
                    Console.WriteLine($"[skill-parity] SkillNodeData มี '{skillId}' แต่ไฟล์สกิลฝั่ง client ไม่รู้จัก");
                }
            }
        }

        Console.WriteLine($"[skill-parity] โหนด {SkillNodeData.Map.Count} · สกิลในตาราง client {SkillData.SkillCategory.Count} · " +
            $"client-ไม่มีโหนด {missingNodes} · โหนด-ไม่มีใน client {extraNodes}");
        if (missingNodes == 0 && extraNodes == 0)
        {
            Console.WriteLine("[skill-parity] ตารางสกิลสองฝั่งตรงกัน");
        }
    }

    /// <summary>เลเวลโหนดสูงสุดของสกิลนี้ (0 = ไม่มีโหนด) — ใช้ตอนส่งแพ็กเก็ต Skills</summary>
    public static int MaxNodeLevel(string skillId, string subId)
    {
        int max = 0;
        string prefix = (skillId ?? "") + "|" + (string.IsNullOrEmpty(subId) ? "__base__" : subId) + "|";
        foreach (KeyValuePair<string, SkillNodeData.Node> pair in SkillNodeData.Map)
        {
            if (pair.Key.StartsWith(prefix, StringComparison.Ordinal) && pair.Value.Level > max)
            {
                max = pair.Value.Level;
            }
        }
        return max;
    }
}
