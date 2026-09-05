using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json.Linq;

namespace DurangoServer.Core;

/// <summary>
/// อาชีพตอนสร้างตัว — อ่านจาก data/assets/player/jobs.json (ชุดเดียวกับเกมจริง)
/// ดันความชำนาญหมวดต้นทางเป็น 20 + ปลดโหนดสกิลที่อาชีพนั้นได้มา
/// </summary>
public static class JobCatalog
{
    public sealed class Grant
    {
        public string SkillId = "";
        public string SubId = "__base__";
        public int Level = 1;
    }

    public sealed class Definition
    {
        public Dictionary<int, int> CategoryLevels = new Dictionary<int, int>();
        public List<Grant> GivenSkills = new List<Grant>();
    }

    private static readonly Dictionary<int, Definition> _jobs = new Dictionary<int, Definition>();
    private static bool _loaded;

    public static void Load(string dataDir)
    {
        _jobs.Clear();
        _loaded = false;
        string path = Path.Combine(dataDir, "assets", "player", "jobs.json");
        if (!File.Exists(path))
        {
            Console.WriteLine("[jobs] ไม่พบ " + path + " — จะใช้ตารางสำรองใน CharacterService");
            return;
        }
        try
        {
            JObject root = JObject.Parse(File.ReadAllText(path));
            foreach (KeyValuePair<string, JToken?> pair in root)
            {
                if (!int.TryParse(pair.Key, out int job) || pair.Value is not JObject obj)
                {
                    continue;
                }
                var def = new Definition();
                JObject? levels = obj["category_levels"] as JObject;
                if (levels != null)
                {
                    foreach (KeyValuePair<string, JToken?> lv in levels)
                    {
                        if (int.TryParse(lv.Key, out int cat) && lv.Value != null)
                        {
                            def.CategoryLevels[cat] = (int)lv.Value;
                        }
                    }
                }
                if (obj["given_skills"] is JArray skills)
                {
                    foreach (JToken row in skills)
                    {
                        if (row is not JArray arr || arr.Count < 2)
                        {
                            continue;
                        }
                        def.GivenSkills.Add(new Grant
                        {
                            SkillId = (string?)arr[0] ?? "",
                            Level = arr[1]?.Value<int>() ?? 1,
                            SubId = arr.Count > 2 ? ((string?)arr[2] ?? "__base__") : "__base__"
                        });
                    }
                }
                _jobs[job] = def;
            }
            _loaded = true;
            Console.WriteLine($"[jobs] โหลด {_jobs.Count} อาชีพจาก {path}");
        }
        catch (Exception e)
        {
            Console.WriteLine("[jobs] อ่าน jobs.json ไม่สำเร็จ: " + e.Message);
        }
    }

    public static bool TryGet(int job, out Definition def) => _jobs.TryGetValue(job, out def!);

    public static bool Loaded => _loaded;
}
