using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace DurangoServer.Core;

// ============================================================================
// RecipeJsonLoader — อ่านสูตรคราฟต์จาก data/assets/item/recipes.json "ตอนรัน"
// (4 ก.ย. 2026, เจ้าของสั่ง "ให้ทุกเครื่องใช้ไฟล์เดียวกัน")
//
// ทำไม: เซิร์ฟเสิร์ฟ recipes.json ไฟล์นี้ให้ client ทุกคนผ่าน /assets/item/recipes
// (โหมด Online) แต่ตรรกะเซิร์ฟกลับใช้ตาราง C# (RecipeMeta/RecipeRequirements) ที่สกัด
// ไว้ตั้งแต่ build ⇒ ถ้าสองชุดไม่ตรงกัน client เห็นสูตรหนึ่งแต่เซิร์ฟตัดสินอีกอย่าง
// (บั๊ก "ตัดแกน" 3 ก.ย.: client ให้เลือกดาบหินได้ แต่เซิร์ฟบอก "ต้องใช้ขวาน")
//
// ตัวนี้ทำให้ "ไฟล์ JSON ที่ส่งให้ client" = "แหล่งข้อมูลที่เซิร์ฟใช้" เป็นไฟล์เดียวกันจริง ๆ
// อ่านไม่ได้/พัง = คงตาราง C# เดิมไว้ (fallback) เซิร์ฟไม่ล้ม
//
// รูปแบบ JSON (ต่อ 1 สูตร): category/subcategory/energy/duration/count/min_level/type/
//   prototype_id · workbench_tags{tag:level} · tool_tags{tag:level} ·
//   slots[]{slot_id,count_min,count_max,required_tags{},required_materials{}} ·
//   prototypes[]{prototype_id,criteria[]{slot_id,tag_id,condition}}
// ============================================================================

public static class RecipeJsonLoader
{
    /// <summary>sha256 ของ recipes.json ที่โหลดสำเร็จล่าสุด (ไว้เทียบว่าทุกฝั่งใช้ไฟล์เดียวกัน)</summary>
    public static string LoadedSha256 { get; private set; }

    /// <summary>จำนวนสูตรที่อ่านจาก JSON ได้ (0 = ยังไม่โหลด / ใช้ fallback)</summary>
    public static int LoadedRecipeCount { get; private set; }

    /// <summary>
    /// อ่าน recipes.json แล้วเขียนทับ RecipeMeta.Map + RecipeRequirements.Recipes
    /// คืน true ถ้าโหลดได้ (false = ใช้ตาราง C# เดิมต่อ)
    /// </summary>
    public static bool LoadInto(string recipesJsonPath)
    {
        if (string.IsNullOrEmpty(recipesJsonPath) || !File.Exists(recipesJsonPath))
        {
            Console.WriteLine("[recipe-json] ไม่พบ {0} — ใช้ตารางคราฟต์ที่ build ไว้ (fallback)", recipesJsonPath);
            return false;
        }
        try
        {
            string text = File.ReadAllText(recipesJsonPath);
            JObject root = JObject.Parse(text);

            var metaMap = new Dictionary<string, RecipeMeta.Info>(RecipeMeta.Map);
            var reqMap = new Dictionary<string, RecipeRequirements.Slot[]>(RecipeRequirements.Recipes);
            int n = 0;
            foreach (KeyValuePair<string, JToken> kv in root)
            {
                if (kv.Value is not JObject r) { continue; }
                string id = kv.Key;
                metaMap[id] = BuildInfo(id, r);
                reqMap[id] = BuildSlots(r);
                n++;
            }

            RecipeMeta.Map = metaMap;
            RecipeRequirements.Recipes = reqMap;
            LoadedRecipeCount = n;
            LoadedSha256 = Sha256Of(text);
            Console.WriteLine("[recipe-json] อ่าน {0} สูตรจาก recipes.json แล้ว (sha {1}) — เซิร์ฟ+client ใช้ไฟล์เดียวกัน",
                n, LoadedSha256.Substring(0, 12));
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine("[recipe-json] อ่าน recipes.json ไม่สำเร็จ ({0}) — คงตารางเดิมไว้ (fallback)", e.Message);
            return false;
        }
    }

    private static RecipeMeta.Info BuildInfo(string id, JObject r)
    {
        string category = (string)r["category"];
        string subcategory = (string)r["subcategory"];
        float duration = Num(r["duration"]);
        float energy = Num(r["energy"]);
        int count = (int)Num(r["count"]);
        if (count <= 0) { count = 1; }
        int minLevel = (int)Num(r["min_level"]);
        if (minLevel <= 0) { minLevel = 1; }
        int type = (int)Num(r["type"]);
        string prototypeId = (string)r["prototype_id"] ?? id;

        RecipeMeta.Tag[] workbench = Tags(r["workbench_tags"] as JObject);
        RecipeMeta.Tag[] tools = Tags(r["tool_tags"] as JObject);
        RecipeMeta.Output[] outputs = Outputs(r["prototypes"] as JArray);

        return new RecipeMeta.Info(category, subcategory, duration, energy, count,
            minLevel, type, prototypeId, workbench, tools, outputs);
    }

    /// <summary>{tag:level} → Tag[] (เรียงชื่อแบบ ordinal ให้ตรงกับตัว generate เดิม) · ว่าง = null</summary>
    private static RecipeMeta.Tag[] Tags(JObject dic)
    {
        if (dic == null || !dic.HasValues) { return null; }
        return dic.Properties()
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => new RecipeMeta.Tag(p.Name, (int)Num(p.Value)))
            .ToArray();
    }

    private static RecipeMeta.Output[] Outputs(JArray protos)
    {
        if (protos == null || protos.Count == 0) { return null; }
        var list = new List<RecipeMeta.Output>();
        foreach (JToken pt in protos)
        {
            if (pt is not JObject p) { continue; }
            RecipeMeta.Criterion[] crit = null;
            if (p["criteria"] is JArray ca && ca.Count > 0)
            {
                crit = ca.OfType<JObject>().Select(c => new RecipeMeta.Criterion(
                    (string)c["slot_id"], (string)c["tag_id"], (string)c["condition"] ?? ">0")).ToArray();
            }
            list.Add(new RecipeMeta.Output((string)p["prototype_id"], crit));
        }
        return list.Count == 0 ? null : list.ToArray();
    }

    private static RecipeRequirements.Slot[] BuildSlots(JObject r)
    {
        if (r["slots"] is not JArray slots || slots.Count == 0) { return Array.Empty<RecipeRequirements.Slot>(); }
        var list = new List<RecipeRequirements.Slot>();
        foreach (JToken st in slots)
        {
            if (st is not JObject s) { continue; }
            string slotId = (string)s["slot_id"];
            int min = (int)Num(s["count_min"]);
            int max = (int)Num(s["count_max"]);
            TagRequirement[] tags = Reqs(s["required_tags"] as JObject);
            TagRequirement[] mats = Reqs(s["required_materials"] as JObject);
            list.Add(new RecipeRequirements.Slot(slotId, min, max, tags, mats));
        }
        return list.ToArray();
    }

    /// <summary>{tag:level} → TagRequirement[] (เรียง ordinal) · ว่าง = null</summary>
    private static TagRequirement[] Reqs(JObject dic)
    {
        if (dic == null || !dic.HasValues) { return null; }
        return dic.Properties()
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => new TagRequirement(p.Name, (int)Num(p.Value)))
            .ToArray();
    }

    /// <summary>ค่าในข้อมูลเกมมีทั้ง "9" (string) และ 5 (int) และบางช่องเป็นสูตร/ว่าง</summary>
    private static float Num(JToken t)
    {
        if (t == null || t.Type == JTokenType.Null) { return 0f; }
        if (t.Type == JTokenType.Integer || t.Type == JTokenType.Float) { return (float)t; }
        string s = (string)t;
        return float.TryParse(s, NumberStyles.Float, CultureInfo.InvariantCulture, out float v) ? v : 0f;
    }

    private static string Sha256Of(string text)
    {
        using SHA256 sha = SHA256.Create();
        byte[] hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(text));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
