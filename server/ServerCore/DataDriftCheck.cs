using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DurangoServer.Core;

// ============================================================================
// DataDriftCheck — ตรวจว่าตารางที่ hardcode ใน C# ยังตรงกับข้อมูลของเกมไหม
//
// ทำไมต้องมี: เซิร์ฟ **ไม่เคยอ่าน** `data/assets/*.json` ตอนรัน — มันแค่เสิร์ฟให้ client
// (`Gateway.cs` route `/assets/*`) ส่วนตรรกะทั้งหมดใช้ตาราง C# ที่สกัดมาไว้ก่อน ~7,500 บรรทัด
// ⇒ มีข้อมูลชุดเดียวกันอยู่สองก๊อปที่ไม่ผูกกัน แก้ข้างหนึ่งอีกข้างไม่รู้เรื่อง
//
// ตัวนี้ไม่ได้แก้อะไร แค่ไล่เทียบแล้วรายงาน — เอาไว้รันหลังอัปเดตข้อมูลเกม
// หรือก่อนปล่อยเวอร์ชัน เพื่อจับว่ามีอะไรหลุดไปบ้าง
//
//   dotnet run --project server/DurangoServer.csproj -- --data-check
//
// ผลตอนเขียน (3 ก.ย. 2026): สูตรคราฟต์ 720/720 · blueprint 556/556 · อนิเมชันสัตว์ 213/213
// · สัตว์ 213/214 — ที่ขาดคือ 2047 ซึ่งไม่มี prefab ใน client (อยู่ใน KnownAbsent)
// · ไอเทม 2397/2407 — ที่ขาดเป็นของอีเวนต์/ของรางวัลคราฟต์
// ============================================================================

public static class DataDriftCheck
{
    private sealed class Result
    {
        public string Name = "";
        public int Game;
        public int Server;
        public List<string> MissingInServer = new();
        public List<string> ExtraInServer = new();
        public string Note = "";
    }

    /// <summary>
    /// ของที่ "เกมมีในข้อมูล แต่ไม่มีไฟล์จริงให้ใช้" — ไม่นับเป็นของขาด
    ///
    /// ตรวจแล้วว่าเติมเข้าเซิร์ฟไม่ได้จริง ๆ ไม่ใช่แค่ยังไม่ได้ทำ
    /// ถ้าวันหลังเจอไฟล์เพิ่ม ให้เอาออกจากรายการนี้แล้วเติมของจริงเข้าตาราง
    /// </summary>
    private static readonly Dictionary<string, string> KnownAbsent = new(StringComparer.Ordinal)
    {
        // 스밀로돈 (Smilodon) — มีใน entity_types/animal.json และ collectible_names.json
        // แต่ค้นใน StreamingAssets/AssetBundles ทั้ง 4,315 ไฟล์แล้ว **ไม่มี prefab ของมันเลย**
        // client build นี้ไม่ได้แถมโมเดลมา ⇒ ถ้าเติมเข้าตารางแล้วเสก จะไม่มีอะไรให้เรนเดอร์
        { "2047", "ไม่มี prefab ใน client build นี้ (ค้น AssetBundles 4,315 ไฟล์แล้วไม่เจอ)" },
    };

    /// <summary>คืน 0 ถ้าไม่มีอะไรหลุด · 1 ถ้ามี (ใช้เป็น exit code ได้)</summary>
    public static int Run(string dataDir)
    {
        string assets = Path.Combine(dataDir, "assets");
        if (!Directory.Exists(assets))
        {
            Console.WriteLine("[data-check] ไม่พบ {0} — ข้ามการตรวจ", assets);
            return 0;
        }

        var results = new List<Result>
        {
            CheckAnimals(assets),
            CheckAnimalMotions(),
            CheckAnimalStats(),
            CheckAnimalKinds(assets),
            CheckRegionTemplates(assets),
            CheckItemLevels(assets),
            CheckBlueprintEffort(assets),
            CheckItems(assets),
            CheckRecipes(assets),
            CheckBlueprints(assets),
        };

        Console.WriteLine();
        Console.WriteLine("=== ตรวจว่าตาราง C# ตรงกับข้อมูลเกมไหม ===");
        Console.WriteLine("{0,-24} {1,7} {2,7} {3,8}  {4}", "ชั้นข้อมูล", "เกม", "เซิร์ฟ", "ขาด", "หมายเหตุ");
        Console.WriteLine(new string('-', 78));
        int problems = 0;
        var excused = new List<string>();
        foreach (Result r in results)
        {
            if (r == null) { continue; }
            // ของที่รู้อยู่แล้วว่าไม่มีไฟล์ให้ใช้ ไม่นับเป็นปัญหา แต่ยังรายงานแยกไว้
            for (int i = r.MissingInServer.Count - 1; i >= 0; i--)
            {
                if (KnownAbsent.TryGetValue(r.MissingInServer[i], out string why))
                {
                    excused.Add($"{r.Name}: {r.MissingInServer[i]} — {why}");
                    r.MissingInServer.RemoveAt(i);
                }
            }
            int miss = r.MissingInServer.Count;
            if (miss > 0) { problems++; }
            Console.WriteLine("{0,-24} {1,7} {2,7} {3,8}  {4}", r.Name, r.Game, r.Server, miss, r.Note);
        }

        foreach (Result r in results)
        {
            if (r == null || r.MissingInServer.Count == 0) { continue; }
            Console.WriteLine();
            Console.WriteLine("[{0}] เกมมีแต่เซิร์ฟไม่มี {1} รายการ:", r.Name, r.MissingInServer.Count);
            foreach (string s in r.MissingInServer.Take(20))
            {
                Console.WriteLine("    {0}", s);
            }
            if (r.MissingInServer.Count > 20)
            {
                Console.WriteLine("    … อีก {0} รายการ", r.MissingInServer.Count - 20);
            }
        }

        if (excused.Count > 0)
        {
            Console.WriteLine();
            Console.WriteLine("ข้ามไม่นับเป็นของขาด (เกมมีข้อมูลแต่ไม่มีไฟล์ให้ใช้):");
            foreach (string e in excused) { Console.WriteLine("    {0}", e); }
        }

        Console.WriteLine();
        Console.WriteLine(problems == 0
            ? "ผลตรวจ: ตรงกันทุกชั้น"
            : $"ผลตรวจ: มี {problems} ชั้นที่ข้อมูลไม่ครบ (ดูรายการข้างบน)");
        return problems == 0 ? 0 : 1;
    }

    private static JObject Load(string assets, string relative)
    {
        string path = Path.Combine(assets, relative.Replace('/', Path.DirectorySeparatorChar));
        return File.Exists(path) ? JObject.Parse(File.ReadAllText(path)) : null;
    }

    private static Result CheckAnimals(string assets)
    {
        JObject j = Load(assets, "entity_types/animal.json");
        if (j == null) { return null; }
        var game = new HashSet<string>(j.Properties().Select(p => p.Name));
        var server = new HashSet<string>(AnimalData.All.Keys.Select(k => k.ToString()));
        return new Result
        {
            Name = "สัตว์",
            Game = game.Count,
            Server = server.Count,
            MissingInServer = game.Except(server).OrderBy(x => x).ToList(),
            ExtraInServer = server.Except(game).OrderBy(x => x).ToList(),
            Note = "ขาด = เสกไม่ได้ / ไม่มีข้อมูลตัวนั้น",
        };
    }

    private static Result CheckAnimalMotions()
    {
        // สัตว์ทุกตัวที่อยู่ในตารางต้องมีชื่อคลิปอนิเมชัน ไม่งั้นเกิดมาแล้ว **ยืนแข็ง**
        // (client เรียก Anim.CrossFade(motionName) ตรง ๆ — ดู AnimalMotionData.cs)
        var have = new HashSet<ushort>(AnimalMotionData.All.Keys);
        var need = new HashSet<ushort>(AnimalData.All.Keys);
        return new Result
        {
            Name = "อนิเมชันสัตว์",
            Game = need.Count,
            Server = have.Count,
            MissingInServer = need.Except(have).Select(x => x.ToString()).OrderBy(x => x).ToList(),
            Note = "ขาด = สัตว์ตัวนั้นเกิดมาแล้วยืนแข็ง",
        };
    }

    private static Result CheckAnimalStats()
    {
        // สัตว์ทุกตัวในตารางควรมีสูตรพลังรายชนิด (AnimalStatData) ไม่งั้นจะตกไปใช้สูตรกลาง
        var have = new HashSet<ushort>(AnimalStatData.All.Keys);
        var need = new HashSet<ushort>(AnimalData.All.Keys);
        return new Result
        {
            Name = "สูตรพลังสัตว์",
            Game = need.Count,
            Server = have.Count,
            MissingInServer = need.Except(have).Select(x => x.ToString()).OrderBy(x => x).ToList(),
            Note = "ขาด = ตกไปใช้สูตรกลาง (แข็งเท่าแร็ปเตอร์)",
        };
    }

    private static Result CheckAnimalKinds(string assets)
    {
        // [TodoList/08] type/attack_cooltime/combat_level_ranges รายชนิด — ระบบฝูงใช้ตัดสินนิสัยของชนิดนอก config
        JObject j = Load(assets, "entity_types/animal.json");
        if (j == null) { return null; }
        var game = new HashSet<string>(j.Properties().Select(p => p.Name));
        var server = new HashSet<string>(AnimalKindData.All.Keys.Select(k => k.ToString()));
        return new Result
        {
            Name = "ประเภทสัตว์ (kind)",
            Game = game.Count,
            Server = server.Count,
            MissingInServer = game.Except(server).OrderBy(x => x).ToList(),
            Note = "ขาด = ฝูงชนิดนั้นใช้นิสัย/คูลดาวน์กลาง",
        };
    }

    private static Result CheckRegionTemplates(string assets)
    {
        // [TodoList/08] ใบสั่งเกิดสัตว์ต่อเกาะ — นับเฉพาะ template ที่มีฝูง/หลุม (เกาะที่ไม่มีสัตว์เลยไม่ใส่ตาราง)
        JObject j = Load(assets, "region_templates.json");
        if (j == null) { return null; }
        var game = new HashSet<string>();
        foreach (JProperty p in j.Properties())
        {
            JObject t = p.Value as JObject;
            bool hasHerd = false;
            if (t?["herds"] is JObject herds)
            {
                foreach (JProperty g in herds.Properties())
                {
                    if (g.Value["spawns"] is JArray arr && arr.Count > 0) { hasHerd = true; break; }
                }
            }
            int craters = t?["biocoms"]?["craters"]?["total_count"]?.Value<int>() ?? 0;
            if (hasHerd || craters > 0) { game.Add(p.Name); }
        }
        var server = new HashSet<string>(RegionTemplateData.All.Keys);
        return new Result
        {
            Name = "region template",
            Game = game.Count,
            Server = server.Count,
            MissingInServer = game.Except(server).OrderBy(x => x).ToList(),
            Note = "ขาด = เกาะนั้นตกไปใช้ตาราง Spawn แบบเดิม",
        };
    }

    private static Result CheckItemLevels(string assets)
    {
        // [TodoList/02] ช่วงเลเวลของ prototype (min/max) — คราฟต์แล้ว clamp ด้วยค่านี้
        JObject j = Load(assets, "item/prototype_data.json");
        if (j == null) { return null; }
        var game = new HashSet<string>(j.Properties().Select(p => p.Name.Trim(' ', ' ')));
        var server = new HashSet<string>(ItemLevelData.Prototypes.Keys);
        return new Result
        {
            Name = "ช่วงเลเวลไอเทม",
            Game = game.Count,
            Server = server.Count,
            MissingInServer = game.Except(server).OrderBy(x => x).ToList(),
            Note = "ขาด = ไม่ clamp เลเวลผลลัพธ์ของชิ้นนั้น",
        };
    }

    private static Result CheckBlueprintEffort(string assets)
    {
        // [TodoList/04] effort/energy ของ blueprint — เวลาสร้างจริง
        JObject j = Load(assets, "building/blueprints.json");
        if (j == null) { return null; }
        var game = new HashSet<string>(j.Properties().Select(p => p.Name));
        var server = new HashSet<string>(BlueprintEffortData.All.Keys);
        return new Result
        {
            Name = "effort สิ่งปลูกสร้าง",
            Game = game.Count,
            Server = server.Count,
            MissingInServer = game.Except(server).OrderBy(x => x).ToList(),
            Note = "ขาด = ใช้สูตร effort_standard.build แทน",
        };
    }

    private static Result CheckItems(string assets)
    {
        JObject j = Load(assets, "item/prototype_data.json");
        if (j == null) { return null; }
        // ข้อมูลของเกมเองมีคีย์ที่ติดช่องว่าง/nbsp มาด้วย (เช่น "metal_tin ") — ตัดทิ้งก่อนเทียบ
        var game = new HashSet<string>(j.Properties().Select(p => p.Name.Trim(' ', ' ')));
        var server = new HashSet<string>(ItemNameData.Map.Keys);
        List<string> miss = game.Except(server).OrderBy(x => x).ToList();
        return new Result
        {
            Name = "ไอเทม (prototype)",
            Game = game.Count,
            Server = server.Count,
            MissingInServer = miss,
            Note = "ขาด = เสกด้วย give/it ไม่ได้",
        };
    }

    private static Result CheckRecipes(string assets)
    {
        JObject j = Load(assets, "item/recipes.json");
        if (j == null) { return null; }
        var game = new HashSet<string>(j.Properties().Select(p => p.Name));
        // ❗ ต้องเทียบกับ RecipeInfo (ตารางของ *สูตร*) ไม่ใช่ BlueprintType
        // ซึ่งเป็น blueprint -> entity type คนละเรื่องกัน (เทียบผิดตัวรอบแรก ได้ 720 ขาด)
        var server = new HashSet<string>(RecipeData.RecipeInfo.Keys);
        return new Result
        {
            Name = "สูตรคราฟต์",
            Game = game.Count,
            Server = server.Count,
            MissingInServer = game.Except(server).OrderBy(x => x).ToList(),
            Note = "ขาด = คราฟต์สูตรนั้นไม่ได้",
        };
    }

    private static Result CheckBlueprints(string assets)
    {
        JObject j = Load(assets, "building/blueprints.json");
        if (j == null) { return null; }
        var game = new HashSet<string>(j.Properties().Select(p => p.Name));
        var server = new HashSet<string>(BlueprintRequirements.Blueprints.Keys);
        return new Result
        {
            Name = "blueprint สิ่งปลูกสร้าง",
            Game = game.Count,
            Server = server.Count,
            MissingInServer = game.Except(server).OrderBy(x => x).ToList(),
            Note = "ขาด = สร้างสิ่งนั้นไม่ได้",
        };
    }
}
