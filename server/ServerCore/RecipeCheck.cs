using System;
using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// `dotnet run -- --recipe-check` — ตรวจข้อมูลคราฟต์/ทำอาหารโดยไม่ต้องเปิดเซิร์ฟ
///
/// ตอบคำถามที่ตอบไม่ได้ถ้าไม่มีเครื่องมือ:
///   · มีสูตรไหนที่ **คราฟต์ไม่ได้เลยตลอดกาล** เพราะไม่มีโต๊ะตัวไหนให้ tag ที่มันขอไหม
///   · ผู้เล่นใหม่ที่มีแต่ของเริ่มต้น ทำสูตรอะไรได้บ้าง ต้องมีอะไรก่อน
///   · กินของแต่ละอย่างแล้วได้สตามินาเท่าไรจริง ๆ หลังคูณสเกลใน config
/// </summary>
public static class RecipeCheck
{
    public static int Run()
    {
        int problems = 0;
        Console.WriteLine("=== ตรวจข้อมูลคราฟต์/ทำอาหาร ===");
        Console.WriteLine($"สูตรทั้งหมด {RecipeMeta.Map.Count} · อาหาร {FoodData.Map.Count} ชนิด · โต๊ะ {WorkbenchTagData.Map.Count} ชนิด");
        Console.WriteLine();

        // ── 1. หมวดของสูตร ─────────────────────────────────────────────
        var byCategory = new Dictionary<string, int>(StringComparer.Ordinal);
        int needWorkbench = 0;
        int needTool = 0;
        foreach (KeyValuePair<string, RecipeMeta.Info> pair in RecipeMeta.Map)
        {
            string cat = pair.Value.Category ?? "(ไม่ระบุ)";
            byCategory.TryGetValue(cat, out int n);
            byCategory[cat] = n + 1;
            if (pair.Value.Workbench != null && pair.Value.Workbench.Length > 0)
            {
                needWorkbench++;
            }
            if (pair.Value.Tools != null && pair.Value.Tools.Length > 0 && pair.Value.Tools[0].Id != "bare_hands")
            {
                needTool++;
            }
        }
        Console.WriteLine("— หมวดสูตร —");
        foreach (KeyValuePair<string, int> pair in byCategory)
        {
            Console.WriteLine($"  {pair.Key,-22} {pair.Value,4}");
        }
        Console.WriteLine($"  ต้องใช้โต๊ะ {needWorkbench} · ต้องถือเครื่องมือ {needTool}");
        Console.WriteLine();

        // ── 2. สูตรที่ไม่มีโต๊ะรองรับ ────────────────────────────────────
        List<string> unreachable = WorkbenchTagData.FindUnreachableRequirements();
        if (unreachable.Count == 0)
        {
            Console.WriteLine("✅ ทุกสูตรมีโต๊ะที่ทำได้จริง");
        }
        else
        {
            problems += unreachable.Count;
            Console.WriteLine($"❌ มี {unreachable.Count} สูตรที่ไม่มีโต๊ะตัวไหนให้ tag ที่ขอได้:");
            for (int i = 0; i < unreachable.Count && i < 20; i++)
            {
                Console.WriteLine("   " + unreachable[i]);
            }
            if (unreachable.Count > 20)
            {
                Console.WriteLine($"   ... อีก {unreachable.Count - 20} สูตร");
            }
        }
        Console.WriteLine();

        // ── 3. สูตรเริ่มต้นของผู้เล่นใหม่ ─────────────────────────────────
        StarterConfig starter = ServerConfig.Current.Starter;
        var starterBlueprints = new HashSet<string>(starter.Blueprints ?? new List<string>(), StringComparer.Ordinal);
        Console.WriteLine("— สูตรเริ่มต้น: ต้องมีอะไรถึงจะทำได้ —");
        foreach (string recipeId in starter.Recipes ?? new List<string>())
        {
            if (!RecipeMeta.TryGet(recipeId, out RecipeMeta.Info meta))
            {
                problems++;
                Console.WriteLine($"  ❌ {recipeId,-26} ไม่มีสูตรนี้ในข้อมูลเกม");
                continue;
            }
            string wb = Describe(meta.Workbench);
            string tools = Describe(meta.Tools);
            string mark = "  ";
            if (meta.Workbench != null && meta.Workbench.Length > 0)
            {
                // ผู้เล่นใหม่สร้างโต๊ะที่ต้องใช้ได้ไหม (จาก blueprint เริ่มต้น)
                bool covered = false;
                foreach (string bp in starterBlueprints)
                {
                    for (int i = 0; i < meta.Workbench.Length && !covered; i++)
                    {
                        if (WorkbenchTagData.LevelOf(bp, meta.Workbench[i].Id) >= meta.Workbench[i].Level)
                        {
                            covered = true;
                        }
                    }
                    if (covered)
                    {
                        break;
                    }
                }
                if (!covered)
                {
                    // ไม่ใช่ข้อมูลพัง — แค่ต้องไปสร้างโต๊ะที่ดีกว่าก่อน (นี่คือความคืบหน้าของเกม)
                    mark = "⚠ ";
                }
            }
            string kind = meta.Type == 1 ? "แปรรูป" : (meta.Type == 2 ? "แก้ทรง" : "คราฟต์");
            Console.WriteLine($"{mark}{recipeId,-26} {kind} · {meta.Category,-16} โต๊ะ {wb,-18} เครื่องมือ {tools,-24} {meta.Duration:F0} วิ · {meta.Energy:F0} สตามินา · ได้ {meta.Count} ชิ้น");
        }
        Console.WriteLine();

        // ── 4. อาหาร: ได้อะไรจริงหลังคูณสเกล ────────────────────────────
        FoodConfig food = ServerConfig.Current.Food;
        Console.WriteLine($"— อาหาร (สเกล: พลัง ×{food.EnergyScale} · ของดิบ ×{food.RawFoodEnergyScale} · ล้า ×{food.FatigueScale}) —");
        string[] samples = { "meat", "meat_lizard", "fish", "fruit_berry", "wildberry", "broth_meat", "roast_meat", "boiled_meat" };
        for (int i = 0; i < samples.Length; i++)
        {
            if (!FoodData.TryGet(samples[i], 1, out FoodData.Entry e))
            {
                Console.WriteLine($"  {samples[i],-16} (ไม่มีในตารางอาหาร)");
                continue;
            }
            bool raw = ItemTagData.LevelOf(samples[i], "raw_food") > 0;
            float stamina = e.EnergyAt(1) * food.EnergyScale * (raw ? food.RawFoodEnergyScale : 1f);
            float fatigue = Math.Max(0f, -e.Fatigue * food.FatigueScale);
            Console.WriteLine($"  {samples[i],-16} {(raw ? "[ดิบ]" : "     ")} +{stamina,5:F1} สตามินา · ล้า −{fatigue,4:F1} · เลือด +{e.HealthAt(1) * food.HealthScale,4:F1} · ย่อย {e.DigestiveTime} วิ");
        }
        Console.WriteLine();

        // ── 5. ดิบ vs สุก: ทำอาหารแล้วคุ้มไหม ───────────────────────────
        Console.WriteLine("— ดิบ vs สุก (ต้องสุกได้มากกว่าดิบ ไม่งั้นไม่มีเหตุผลให้ทำอาหาร) —");
        string[] rawSamples = { "meat", "meat_lizard", "fish" };
        for (int i = 0; i < rawSamples.Length; i++)
        {
            if (!FoodData.TryGet(rawSamples[i], 1, out FoodData.Entry e))
            {
                continue;
            }
            float rawGain = e.EnergyAt(1) * food.EnergyScale * food.RawFoodEnergyScale;
            float cookedGain = e.EnergyAt(1) * food.EnergyScale;
            if (cookedGain <= rawGain)
            {
                problems++;
                Console.WriteLine($"  ❌ {rawSamples[i],-14} ดิบ {rawGain:F1} ≥ สุก {cookedGain:F1}");
                continue;
            }
            Console.WriteLine($"  {rawSamples[i],-14} ดิบ {rawGain,5:F1} → แปรรูปแล้ว {cookedGain,5:F1} (+{cookedGain - rawGain:F1})");
        }
        Console.WriteLine();

        Console.WriteLine(problems == 0 ? "✅ ผ่านทั้งหมด" : $"❌ เจอปัญหา {problems} ข้อ");
        return problems == 0 ? 0 : 1;
    }

    private static string Describe(RecipeMeta.Tag[] tags)
    {
        if (tags == null || tags.Length == 0)
        {
            return "-";
        }
        var parts = new List<string>(tags.Length);
        for (int i = 0; i < tags.Length; i++)
        {
            parts.Add($"{tags[i].Id} {tags[i].Level}");
        }
        return string.Join("/", parts);
    }
}
