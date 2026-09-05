using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json.Linq;

namespace DurangoServer.Core;

// ============================================================================
// GameData — "ทุกเครื่องใช้ไฟล์เดียวกัน" (4 ก.ย. 2026, เจ้าของสั่ง)
//
// เซิร์ฟเสิร์ฟ data/assets/**/*.json ให้ client ทุกคนผ่าน /assets/* (โหมด Online)
// แต่ตรรกะเซิร์ฟเดิมใช้ตาราง C# ที่สกัดไว้ตอน build ⇒ อาจไม่ตรงกับ JSON ที่ส่ง
// ตัวนี้ทำให้เซิร์ฟ "อ่านจากไฟล์เดียวกับที่ส่ง client" ตอนรัน:
//   · recipes.json      → RecipeMeta + RecipeRequirements   (สูตรคราฟต์)
//   · blueprints.json   → BlueprintRequirements              (วัตถุดิบสิ่งปลูกสร้าง)
// อ่านไฟล์ไหนไม่ได้ = คงตาราง C# ของไฟล์นั้นไว้ (fallback) เซิร์ฟไม่ล้ม
//
// นอกจากนี้สร้าง "manifest" = sha256 ของ JSON ทุกไฟล์ใต้ assets ⇒ client/แอดมิน
// เทียบได้ว่าโหลดข้อมูลชุดเดียวกับเซิร์ฟจริง (เสิร์ฟที่ /assets/manifest)
//
// ตารางที่ยัง "ไม่" ย้ายมา (ไม่มี JSON ต้นทางที่สะอาดใน assets — สกัดจาก dump/bundle):
//   ItemTagData/ItemNameData (tag+ชื่ออยู่คนละ asset, prototype_data ไม่มี level ครบ),
//   RecipeGateData (required_ability ใน JSON เพี้ยน), Animal*/RegionTemplate/Skill*
//   → ยังใช้ตาราง C# + `--data-check` เฝ้าว่ายังตรงกับข้อมูลเกมไหม
// ============================================================================

public static class GameData
{
    /// <summary>manifest: relpath (เช่น "item/recipes") → sha256 ของไฟล์ · null = ยังไม่ได้สร้าง</summary>
    public static IReadOnlyDictionary<string, string> Manifest { get; private set; }

    /// <summary>sha256 ของทั้ง manifest (hash ของ hash ทุกไฟล์) — ตัวเลขเดียวบอก "ชุดข้อมูลนี้"</summary>
    public static string ManifestDigest { get; private set; }

    public static int BlueprintCount { get; private set; }

    /// <summary>
    /// โมเดลเริ่มต้นของ "ช่องวัสดุ" ต่อ blueprint: blueprintId -> (slot_id -> model_key)
    /// มาจาก blueprints.json (slots[].default_look_tag + looks[tag].model_key)
    /// ใช้กับสิ่งปลูกสร้างที่ไม่มี default_look (รูปร่างขึ้นกับวัสดุที่ใส่)
    /// </summary>
    public static Dictionary<string, Dictionary<string, string>> BlueprintSlotLooks { get; private set; }
        = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);

    /// <summary>เรียกครั้งเดียวตอนสตาร์ท หลังตั้ง Gateway.AssetsDir</summary>
    public static void LoadAll(string assetsDir)
    {
        // 1) สูตรคราฟต์ (recipes.json)
        RecipeJsonLoader.LoadInto(Path.Combine(assetsDir, "item", "recipes.json"));

        // 2) วัตถุดิบสิ่งปลูกสร้าง (blueprints.json)
        LoadArtifactModels(Path.Combine(assetsDir, "building", "artifact_models.json"));
        LoadBlueprints(Path.Combine(assetsDir, "building", "blueprints.json"));

        // 3) เงื่อนไขปลดล็อกสูตร/สิ่งปลูกสร้าง (required_ability) — จากไฟล์เดียวกัน
        LoadGates(Path.Combine(assetsDir, "item", "recipes.json"),
                  Path.Combine(assetsDir, "building", "blueprints.json"));

        // 3.5) ตาราง palette สี (colortable.json) — ต้องโหลดก่อน LoadItemData เพราะใช้แปลงชื่อสี→hex
        LoadColorTables(Path.Combine(assetsDir, "colortable.json"));

        // 4) tag + ชื่อไอเทม (prototype_data.json)
        LoadItemData(Path.Combine(assetsDir, "item", "prototype_data.json"));

        // 5) manifest ของทุกไฟล์ที่เสิร์ฟ
        BuildManifest(assetsDir);
    }

    /// <summary>ชื่อโมเดลสิ่งปลูกสร้างที่ client มีจริง (artifact_models.json) — ใช้กันส่งชื่อผี</summary>
    private static HashSet<string> _artifactModels = new HashSet<string>(StringComparer.Ordinal);

    private static void LoadArtifactModels(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) { return; }
        try
        {
            JObject root = JObject.Parse(File.ReadAllText(path));
            var set = new HashSet<string>(StringComparer.Ordinal);
            foreach (KeyValuePair<string, JToken> kv in root) { set.Add(kv.Key); }
            _artifactModels = set;
        }
        catch (Exception e)
        {
            Console.WriteLine("[gamedata] อ่าน artifact_models.json ไม่สำเร็จ ({0})", e.Message);
        }
    }

    /// <summary>
    /// blueprints.json บางอันชี้ชื่อโมเดลเก่าที่ไม่มีแล้ว (เช่น raft_deck_wood ของจริงคือ
    /// raft_01_deck_wood) — ลองเติมเลขลำดับกลับเข้าไป ถ้ายังไม่เจอให้คืน null (ไม่ส่งชื่อผี)
    /// </summary>
    private static string ResolveModelKey(string modelKey)
    {
        if (string.IsNullOrEmpty(modelKey)) { return null; }
        if (_artifactModels.Count == 0 || _artifactModels.Contains(modelKey)) { return modelKey; }
        foreach (string m in _artifactModels)
        {
            // ตัดส่วนที่เป็นตัวเลขล้วนออก แล้วเทียบ เช่น raft_01_deck_wood -> raft_deck_wood
            string[] segs = m.Split('_');
            var keep = new List<string>(segs.Length);
            foreach (string sg in segs)
            {
                bool digits = sg.Length > 0;
                foreach (char c in sg) { if (c < '0' || c > '9') { digits = false; break; } }
                if (!digits) { keep.Add(sg); }
            }
            if (string.Join("_", keep) == modelKey) { return m; }
        }
        return null;
    }

    private static void LoadBlueprints(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Console.WriteLine("[gamedata] ไม่พบ {0} — ใช้ตารางสิ่งปลูกสร้างที่ build ไว้ (fallback)", path);
            return;
        }
        try
        {
            JObject root = JObject.Parse(File.ReadAllText(path));
            var map = new Dictionary<string, BlueprintRequirements.Slot[]>(BlueprintRequirements.Blueprints);
            var slotLooks = new Dictionary<string, Dictionary<string, string>>(StringComparer.Ordinal);
            int n = 0;
            foreach (KeyValuePair<string, JToken> kv in root)
            {
                if (kv.Value is not JObject b) { continue; }
                map[kv.Key] = BlueprintSlots(b);

                // [4 ก.ย. 2026] โมเดลของสิ่งปลูกสร้างแบบ "ช่องวัสดุ" (บั๊ก: สร้างเสร็จแล้วยังเป็นโครงไม้)
                // 54/556 blueprint ไม่มี default_look เพราะรูปร่างมาจากวัสดุที่ใส่ในแต่ละช่อง
                // ข้อมูลบอกครบอยู่แล้ว: slot_id + default_look_tag + looks[tag].model_key
                // เดิมเซิร์ฟมีตาราง hardcode แค่ 8 ตัว ⇒ ที่เหลือ Parts ว่าง = client โชว์นั่งร้านตลอดกาล
                var looks = new Dictionary<string, string>(StringComparer.Ordinal);
                if (b["slots"] is JArray slots)
                {
                    foreach (JToken st in slots)
                    {
                        if (st is not JObject s) { continue; }
                        string slotId = (string)s["slot_id"];
                        string tag = (string)s["default_look_tag"];
                        if (string.IsNullOrEmpty(slotId) || s["looks"] is not JObject lk) { continue; }
                        // ไม่มี default_look_tag ก็เอาตัวแรกที่มี model_key
                        JObject chosen = null;
                        if (!string.IsNullOrEmpty(tag)) { chosen = lk[tag] as JObject; }
                        if (chosen == null)
                        {
                            foreach (JProperty p in lk.Properties())
                            {
                                if (p.Value is JObject o && (string)o["model_key"] != null) { chosen = o; break; }
                            }
                        }
                        string modelKey = ResolveModelKey((string)chosen?["model_key"]);
                        if (!string.IsNullOrEmpty(modelKey)) { looks[slotId] = modelKey; }
                    }
                }
                if (looks.Count > 0) { slotLooks[kv.Key] = looks; }
                n++;
            }
            BlueprintRequirements.Blueprints = map;
            BlueprintSlotLooks = slotLooks;
            BlueprintCount = n;
            Console.WriteLine("[gamedata] อ่าน {0} สิ่งปลูกสร้างจาก blueprints.json แล้ว — เซิร์ฟ+client ใช้ไฟล์เดียวกัน (โมเดลตามช่องวัสดุ {1} แบบ)", n, slotLooks.Count);
        }
        catch (Exception e)
        {
            Console.WriteLine("[gamedata] อ่าน blueprints.json ไม่สำเร็จ ({0}) — คงตารางเดิมไว้", e.Message);
        }
    }

    private static BlueprintRequirements.Slot[] BlueprintSlots(JObject b)
    {
        if (b["slots"] is not JArray slots || slots.Count == 0) { return Array.Empty<BlueprintRequirements.Slot>(); }
        var list = new List<BlueprintRequirements.Slot>();
        foreach (JToken st in slots)
        {
            if (st is not JObject s) { continue; }
            int count = (int)NumOr(s["count"], 0);
            if (count <= 0) { continue; }   // ช่อง count 0 = ไม่นับ (ตรงกับ extract_blueprint_requirements.py)
            list.Add(new BlueprintRequirements.Slot(
                (string)s["slot_id"] ?? "", count, count,
                Reqs(s["required_tags"] as JObject), Reqs(s["required_materials"] as JObject)));
        }
        return list.ToArray();
    }

    /// <summary>{tag:level} → TagRequirement[] (เรียง ordinal, null/0 = level 1) · ว่าง = null</summary>
    private static TagRequirement[] Reqs(JObject dic)
    {
        if (dic == null || !dic.HasValues) { return null; }
        return dic.Properties()
            .OrderBy(p => p.Name, StringComparer.Ordinal)
            .Select(p => new TagRequirement(p.Name, Math.Max(1, (int)NumOr(p.Value, 1))))
            .ToArray();
    }

    private static float NumOr(JToken t, float fallback)
    {
        if (t == null || t.Type == JTokenType.Null) { return fallback; }
        if (t.Type == JTokenType.Integer || t.Type == JTokenType.Float) { return (float)t; }
        return float.TryParse((string)t, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : fallback;
    }

    public static int RecipeGateCount { get; private set; }
    public static int BlueprintGateCount { get; private set; }
    public static int ItemTagCount { get; private set; }
    public static int ItemNameCount { get; private set; }

    /// <summary>สีจริงของไอเทมจาก prototype_data (color_r/g/b เป็นชื่อสี) — ว่าง = ยังไม่โหลด</summary>
    private static Dictionary<string, (string R, string G, string B)> _itemColors;

    /// <summary>สีของ prototype นี้ (ชื่อสี) — คืน (null,null,null) ถ้าไม่มี ให้ผู้เรียก fallback เอง</summary>
    public static (string R, string G, string B) ItemColor(string prototype)
    {
        if (_itemColors != null && prototype != null && _itemColors.TryGetValue(prototype, out var c))
        {
            return c;
        }
        return (null, null, null);
    }

    /// <summary>สีของ prototype (แต่ละช่องที่ว่าง = "FFFFFF") — ใช้ตอนสร้าง Item message</summary>
    public static (string R, string G, string B) ItemColorOrWhite(string prototype)
    {
        var (r, g, b) = ItemColor(prototype);
        return (ResolveColor(r, prototype) ?? "FFFFFF",
                ResolveColor(g, prototype) ?? "FFFFFF",
                ResolveColor(b, prototype) ?? "FFFFFF");
    }

    // ── ตาราง palette สี (colortable.json สร้างจาก tools/make-colortable.py) ─────────
    /// <summary>ชื่อ palette → ลิสต์สี hex 6 หลัก · ว่าง = ยังไม่โหลด</summary>
    private static Dictionary<string, string[]> _colorTables;

    private static void LoadColorTables(string path)
    {
        if (!File.Exists(path))
        {
            Console.WriteLine("[gamedata] ไม่พบ {0} — ไอเทมที่ใช้ชื่อ palette จะเป็นสีขาว", path);
            return;
        }
        try
        {
            JObject root = JObject.Parse(File.ReadAllText(path));
            var map = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, JToken> kv in root)
            {
                if (kv.Value is not JArray arr || arr.Count == 0) { continue; }
                var list = new List<string>(arr.Count);
                foreach (JToken t in arr)
                {
                    string hex = NormalizeHex((string)t);
                    if (hex != null) { list.Add(hex); }
                }
                if (list.Count > 0) { map[kv.Key] = list.ToArray(); }
            }
            _colorTables = map;
            Console.WriteLine("[gamedata] โหลด palette สี {0} ชุด", map.Count);
        }
        catch (Exception e)
        {
            Console.WriteLine("[gamedata] อ่าน colortable.json ไม่สำเร็จ: " + e.Message);
        }
    }

    /// <summary>
    /// ทำ hex ให้อยู่รูปที่ client รับได้ — <c>StringExtensions.ToColor</c> รับ **เฉพาะ 6 หรือ 8 ตัวอักษร**
    /// นอกนั้นคืน Color.white ⇒ "#5F574C" (7 ตัว) ก็ขาว! ต้องตัด '#' ทิ้งเสมอ
    /// </summary>
    private static string NormalizeHex(string s)
    {
        if (string.IsNullOrWhiteSpace(s)) { return null; }
        s = s.Trim();
        if (s.StartsWith("#", StringComparison.Ordinal)) { s = s.Substring(1); }
        if (s.Length != 6 && s.Length != 8) { return null; }
        for (int i = 0; i < s.Length; i++)
        {
            if (Uri.IsHexDigit(s[i])) { continue; }
            return null;
        }
        return s.ToUpperInvariant();
    }

    /// <summary>
    /// แปลงค่าสีจากข้อมูลเกมเป็น hex ที่ client ใช้ได้จริง
    ///   · เป็น hex อยู่แล้ว ("#5F574C" / "5F574C") → ตัด '#' คืน 6 ตัว
    ///   · เป็นชื่อ palette ("color_wood") → สุ่มแบบคงที่จาก seed (prototype) ให้ไอเทมชนิดเดิมสีเดิมเสมอ
    ///   · อย่างอื่น → null (ผู้เรียก fallback เป็นขาว)
    /// </summary>
    public static string ResolveColor(string value, string seed)
    {
        string hex = NormalizeHex(value);
        if (hex != null) { return hex; }
        if (string.IsNullOrWhiteSpace(value) || _colorTables == null) { return null; }

        string key = value.Trim();
        if (key.EndsWith(".raw", StringComparison.OrdinalIgnoreCase))
        {
            key = key.Substring(0, key.Length - 4);
        }
        if (!_colorTables.TryGetValue(key, out string[] palette) || palette.Length == 0)
        {
            return null;
        }
        // hash คงที่ (ไม่ใช้ string.GetHashCode เพราะ .NET randomize ต่อ process ⇒ สีเปลี่ยนทุกครั้งที่รีสตาร์ต)
        uint h = 2166136261u;
        string s = seed ?? key;
        for (int i = 0; i < s.Length; i++)
        {
            h = (h ^ s[i]) * 16777619u;
        }
        return palette[(int)(h % (uint)palette.Length)];
    }

    // ── เงื่อนไขปลดล็อก (required_ability) ────────────────────────────────────
    private static readonly System.Text.RegularExpressions.Regex CoefRe =
        new System.Text.RegularExpressions.Regex(@"[0-9]+(?:\.[0-9]+)?");

    /// <summary>
    /// ตัวคูณจาก required_ability_value — รองรับทั้ง "0.5 * level" และ "level * 0.5"
    /// (ต้นฉบับมีทั้งสองลำดับ!) รวมทั้งตัวเลขล้วน · คืน 0 = ไม่มีเงื่อนไข
    /// </summary>
    private static float GateCoef(JToken t)
    {
        if (t == null || t.Type == JTokenType.Null) { return 0f; }
        if (t.Type == JTokenType.Integer || t.Type == JTokenType.Float) { return (float)t; }
        var m = CoefRe.Match((string)t ?? "");
        return m.Success && float.TryParse(m.Value, System.Globalization.NumberStyles.Float,
            System.Globalization.CultureInfo.InvariantCulture, out float v) ? v : 0f;
    }

    private static Dictionary<string, (int, float)> BuildGates(JObject root)
    {
        var map = new Dictionary<string, (int, float)>();
        foreach (KeyValuePair<string, JToken> kv in root)
        {
            if (kv.Value is not JObject r) { continue; }
            JToken a = r["required_ability"];
            if (a == null || a.Type == JTokenType.Null) { continue; }
            float coef = GateCoef(r["required_ability_value"]);
            if (coef <= 0f) { continue; }                 // value 0 = ไม่มีเงื่อนไข
            map[kv.Key] = ((int)(float)a, coef);
        }
        return map;
    }

    private static void LoadGates(string recipesPath, string blueprintsPath)
    {
        try
        {
            if (File.Exists(recipesPath))
            {
                var g = BuildGates(JObject.Parse(File.ReadAllText(recipesPath)));
                if (g.Count > 0) { RecipeGateData.Required = g; RecipeGateCount = g.Count; }
            }
            if (File.Exists(blueprintsPath))
            {
                var g = BuildGates(JObject.Parse(File.ReadAllText(blueprintsPath)));
                if (g.Count > 0) { BlueprintGateData.Required = g; BlueprintGateCount = g.Count; }
            }
            Console.WriteLine("[gamedata] เงื่อนไขปลดล็อก: สูตร {0} · สิ่งปลูกสร้าง {1} (required_ability จาก JSON)",
                RecipeGateCount, BlueprintGateCount);
        }
        catch (Exception e)
        {
            Console.WriteLine("[gamedata] อ่านเงื่อนไขปลดล็อกไม่สำเร็จ ({0}) — คงตารางเดิม", e.Message);
        }
    }

    // ── tag + ชื่อไอเทม (prototype_data.json) ─────────────────────────────────
    // ชนิดเครื่องมือที่เซิร์ฟแยกเอง (พอร์ตจาก scripts/extract_item_tags.py — ต้องตรงกัน)
    private static readonly (string word, string tag)[] ToolKinds =
    {
        ("pickaxe", "pickaxe"), ("shovel", "shovel"), ("hammer", "hammer"),
        ("axe", "axe"), ("knife", "knife"), ("sword_tool", "knife"), ("sickle", "sickle"),
    };
    private static readonly (string mat, int lvl)[] MaterialLevel =
        { ("metal", 3), ("bone", 2), ("stone", 1), ("wooden", 1), ("wood", 1) };

    private static void LoadItemData(string prototypeDataPath)
    {
        if (!File.Exists(prototypeDataPath))
        {
            Console.WriteLine("[gamedata] ไม่พบ {0} — ใช้ตาราง tag/ชื่อไอเทมที่ build ไว้", prototypeDataPath);
            return;
        }
        try
        {
            JObject root = JObject.Parse(File.ReadAllText(prototypeDataPath));
            var tagMap = new Dictionary<string, Messages.Tag[]>(ItemTagData.Map);
            var nameMap = new Dictionary<string, (string, string)>(ItemNameData.Map, StringComparer.Ordinal);
            var colorMap = new Dictionary<string, (string, string, string)>(StringComparer.Ordinal);
            int nt = 0, nn = 0, nc = 0;
            foreach (KeyValuePair<string, JToken> kv in root)
            {
                // prototype_data: value เป็น array (เอาตัวแรก)
                JObject item = (kv.Value as JArray)?.OfType<JObject>().FirstOrDefault() ?? kv.Value as JObject;
                if (item == null) { continue; }
                string proto = kv.Key;

                // ชื่อ (ภาษาเกาหลี = key แรกของ dict "name") + icon
                string name = (item["name"] as JObject)?.Properties().FirstOrDefault()?.Name ?? "";
                string icon = (string)item["icon"] ?? "";
                nameMap[proto] = (name, icon);
                nn++;

                // สี (color_r/g/b เป็นชื่อสี เช่น "color_rock_stone") — เดิมเซิร์ฟส่ง FFFFFF ⇒ ไอเทมขาวหมด
                string cr = (string)item["color_r"], cg = (string)item["color_g"], cb = (string)item["color_b"];
                if (!string.IsNullOrEmpty(cr) || !string.IsNullOrEmpty(cg) || !string.IsNullOrEmpty(cb))
                {
                    colorMap[proto] = (cr, cg, cb);
                    nc++;
                }

                // tag: {tag: level_or_null} → null = 1 · แล้วเติม tag เครื่องมือแบบเดียวกับสคริปต์
                if (item["tags"] is JObject tags && tags.HasValues)
                {
                    var d = new Dictionary<string, int>(StringComparer.Ordinal);
                    foreach (var p in tags.Properties())
                    {
                        d[p.Name] = p.Value.Type == JTokenType.Null ? 1 : Math.Max(1, (int)NumOr(p.Value, 1));
                    }
                    ExpandToolTags(proto, d);
                    tagMap[proto] = d.OrderBy(x => x.Key, StringComparer.Ordinal)
                        .Select(x => new Messages.Tag { Id = x.Key, Level = x.Value }).ToArray();
                    nt++;
                }
            }
            ItemTagData.Map = tagMap;
            ItemNameData.Map = nameMap;
            _itemColors = colorMap;
            ItemTagCount = nt; ItemNameCount = nn;
            Console.WriteLine("[gamedata] อ่าน tag {0} + ชื่อ {1} + สี {2} ไอเทมจาก prototype_data.json แล้ว", nt, nn, nc);
        }
        catch (Exception e)
        {
            Console.WriteLine("[gamedata] อ่าน prototype_data.json ไม่สำเร็จ ({0}) — คงตารางเดิม", e.Message);
        }
    }

    private static void ExpandToolTags(string proto, Dictionary<string, int> tags)
    {
        foreach (var (word, tag) in ToolKinds)
        {
            if (!proto.Contains(word, StringComparison.Ordinal)) { continue; }
            if (!tags.ContainsKey(tag))
            {
                int lvl = 1;
                foreach (var (mat, n) in MaterialLevel)
                {
                    if (proto.Contains(mat, StringComparison.Ordinal)) { lvl = n; break; }
                }
                tags[tag] = lvl;
                if (!tags.ContainsKey("tool")) { tags["tool"] = lvl; }
            }
            break;   // ตรงกับสคริปต์: เจอชนิดแรกแล้วหยุด
        }
    }

    private static void BuildManifest(string assetsDir)
    {
        try
        {
            var man = new SortedDictionary<string, string>(StringComparer.Ordinal);
            string root = Path.GetFullPath(assetsDir);
            foreach (string file in Directory.EnumerateFiles(root, "*.json", SearchOption.AllDirectories))
            {
                string rel = Path.GetRelativePath(root, file).Replace('\\', '/');
                if (rel.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
                {
                    rel = rel.Substring(0, rel.Length - 5);   // client ขอโดยไม่มี .json
                }
                man[rel] = Sha256File(file);
            }
            Manifest = man;
            // digest รวม = sha256 ของ "relpath:sha\n" ทุกบรรทัด (เรียงแล้ว)
            var sb = new StringBuilder();
            foreach (var kv in man) { sb.Append(kv.Key).Append(':').Append(kv.Value).Append('\n'); }
            ManifestDigest = Sha256Bytes(Encoding.UTF8.GetBytes(sb.ToString()));
            Console.WriteLine("[gamedata] manifest: {0} ไฟล์ · digest {1} (client เทียบที่ /assets/manifest)",
                man.Count, ManifestDigest.Substring(0, 12));
        }
        catch (Exception e)
        {
            Console.WriteLine("[gamedata] สร้าง manifest ไม่สำเร็จ: " + e.Message);
        }
    }

    private static string Sha256File(string path)
    {
        using SHA256 sha = SHA256.Create();
        using FileStream fs = File.OpenRead(path);
        return Convert.ToHexString(sha.ComputeHash(fs)).ToLowerInvariant();
    }

    private static string Sha256Bytes(byte[] b)
    {
        using SHA256 sha = SHA256.Create();
        return Convert.ToHexString(sha.ComputeHash(b)).ToLowerInvariant();
    }
}
