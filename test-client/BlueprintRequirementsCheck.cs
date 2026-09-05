using System;
using System.Collections.Generic;
using DurangoServer.Core;

namespace DurangoTestClient;

/// <summary>ตรวจความครบถ้วนของข้อมูลวัตถุดิบ blueprint ที่ server ใช้บังคับตอนก่อสร้าง</summary>
public static class BlueprintRequirementsCheck
{
    private static int _passed;
    private static int _failed;

    private static void Check(string name, bool ok, string detail = null)
    {
        if (ok)
        {
            _passed++;
            Console.WriteLine($"  [ผ่าน] {name}{(detail == null ? "" : " — " + detail)}");
        }
        else
        {
            _failed++;
            Console.WriteLine($"  [ตก ] {name}{(detail == null ? "" : " — " + detail)}");
        }
    }

    public static int Run()
    {
        Console.WriteLine("=== blueprint requirements check ===");
        var known = new HashSet<string>(RecipeData.AllBlueprintIds, StringComparer.Ordinal);
        int missing = 0;
        int invalid = 0;
        int slots = 0;

        foreach (string blueprintId in RecipeData.AllBlueprintIds)
        {
            if (!BlueprintRequirements.TryGet(blueprintId, out BlueprintRequirements.Slot[] requirements))
            {
                missing++;
                continue;
            }
            var ids = new HashSet<string>(StringComparer.Ordinal);
            foreach (BlueprintRequirements.Slot slot in requirements)
            {
                slots++;
                if (slot == null || string.IsNullOrEmpty(slot.Id) || !ids.Add(slot.Id)
                    || slot.Min < 0 || slot.Max < slot.Min)
                {
                    invalid++;
                    continue;
                }
                foreach (TagRequirement tag in slot.Tags ?? Array.Empty<TagRequirement>())
                    if (string.IsNullOrEmpty(tag.Id) || tag.Level < 1) invalid++;
                foreach (TagRequirement material in slot.Materials ?? Array.Empty<TagRequirement>())
                    if (string.IsNullOrEmpty(material.Id) || material.Level < 1) invalid++;
            }
        }

        Check("blueprint ที่ server รู้จักทุกอันมี requirement entry", missing == 0, "missing=" + missing);
        Check("slot requirement ทุกอันมี id และ min/max ถูกต้อง", invalid == 0, "invalid=" + invalid);
        Check("มีช่องวัตถุดิบ generated", slots > 0, "slots=" + slots);

        foreach (string fixture in new[] { "bonfire", "fur_box_03_leaf", "worktable_05" })
        {
            BlueprintRequirements.Slot[] requirements = null;
            bool exists = known.Contains(fixture) && BlueprintRequirements.TryGet(fixture, out requirements)
                && requirements != null;
            Check("fixture " + fixture + " มี requirement", exists, exists ? "slots=" + requirements.Length : null);
        }

        Console.WriteLine($"=== blueprint requirements result: PASS {_passed}, FAIL {_failed} ===");
        return _failed == 0 ? 0 : 1;
    }
}
