using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace DurangoServer.Core;

// ============================================================================
// TerrainYaml — อ่าน pois.yml / herds.yml ที่มากับเกาะของเกม
//
// ทำไมต้องเขียนเอง: โปรเจกต์ไม่มีไลบรารี YAML (info.yml เป็น JSON เลยใช้ Newtonsoft ได้
// แต่ pois.yml/herds.yml เป็น YAML จริง ๆ) และไฟล์สองตัวนี้ใช้ YAML แค่ subset เล็ก ๆ:
//   - block mapping        key:
//   - block sequence       - item
//   - ค่าว่างแบบ inline    key: {}   /   key: []
//   - scalar เป็นเลขล้วน
// ไม่มี anchor/alias, ไม่มี multi-line string, ไม่มี flow mapping ที่มีข้อมูลจริง
// ⇒ parser 1 หน้าพอ และเช็คได้ครบทุกเคสที่ไฟล์จริงมี
//
// ข้อมูลที่ได้ (ยืนยันจากเกาะจริงทั้ง 13 ใบ):
//   pois.yml   warpholes 5 · rifts 2 · port_points 1 · camp_artifacts 1 ต่อเกาะ
//              **ไม่มีจุดไหนตกน้ำหรือตกในหินเลย** และ port_points ตรงกับ entry_points เป๊ะ
//   herds.yml  จุดเกิดสัตว์แบ่งตามถิ่นที่อยู่ (beach/land/ocean/lake_shallow/lake_deep)
//              ri35te มี 811 จุด · **ไม่มีจุดไหนอยู่ในหิน**
// ============================================================================

/// <summary>ก้อนข้อมูล YAML ก้อนหนึ่ง — เป็น map, list หรือ scalar อย่างใดอย่างหนึ่ง</summary>
internal sealed class YamlNode
{
    public Dictionary<string, YamlNode>? Map;
    public List<YamlNode>? List;
    public string? Scalar;

    public YamlNode? Get(string key)
    {
        return Map != null && Map.TryGetValue(key, out YamlNode? n) ? n : null;
    }

    /// <summary>อ่านเป็นลิสต์ (ถ้าเป็น map จะคืนค่าของทุกคีย์ — pois.yml เก็บ warpholes แบบ `0:` `1:`)</summary>
    public IEnumerable<YamlNode> AsList()
    {
        if (List != null)
        {
            foreach (YamlNode n in List) { yield return n; }
        }
        else if (Map != null)
        {
            foreach (YamlNode n in Map.Values) { yield return n; }
        }
    }

    public bool TryInt(out int value)
    {
        return int.TryParse(Scalar, NumberStyles.Integer, CultureInfo.InvariantCulture, out value);
    }

    /// <summary>อ่านเป็นคู่พิกัด [x, y]</summary>
    public bool TryPoint(out int x, out int y)
    {
        x = y = 0;
        if (List == null || List.Count < 2) { return false; }
        return List[0].TryInt(out x) && List[1].TryInt(out y);
    }
}

internal static class TerrainYaml
{
    /// <summary>อ่านไฟล์ YAML — คืน null ถ้าไม่มีไฟล์หรืออ่านไม่ได้ (ผู้เรียกต้องเผื่อกรณีเกาะที่ปั่นเอง)</summary>
    public static YamlNode? Load(string path)
    {
        if (!File.Exists(path)) { return null; }
        try
        {
            string[] lines = File.ReadAllLines(path);
            int i = 0;
            return ParseBlock(lines, ref i, 0);
        }
        catch (Exception e)
        {
            Console.WriteLine("[terrain] อ่าน {0} ไม่ได้: {1}", Path.GetFileName(path), e.Message);
            return null;
        }
    }

    private static bool IsSkippable(string line)
    {
        string t = line.TrimStart();
        return t.Length == 0 || t[0] == '#' || t == "---";
    }

    private static int IndentOf(string line)
    {
        int n = 0;
        while (n < line.Length && line[n] == ' ') { n++; }
        return n;
    }

    /// <summary>
    /// อ่านบล็อกหนึ่งที่ระดับย่อหน้า minIndent ขึ้นไป
    ///
    /// บล็อกหนึ่งเป็นได้อย่างเดียว — list หรือ map — ดูจากบรรทัดแรกที่เจอ
    /// พอชนิดเปลี่ยนถือว่าจบบล็อก (เคสจริง: `warpholes:` มีคีย์ `0:` ที่ค่าเป็นลิสต์
    /// วางย่อหน้าเท่ากับคีย์ ถ้าไม่ตัดตรงนี้ ลิสต์ของ `0:` จะกลืนคีย์ `1:` เข้าไปด้วย)
    /// </summary>
    private static YamlNode ParseBlock(string[] lines, ref int i, int minIndent)
    {
        var node = new YamlNode();
        bool? isList = null;

        while (i < lines.Length)
        {
            string raw = lines[i];
            if (IsSkippable(raw)) { i++; continue; }

            int indent = IndentOf(raw);
            if (indent < minIndent) { break; }

            string text = raw.Substring(indent);
            bool seq = text == "-" || text.StartsWith("- ", StringComparison.Ordinal);
            if (isList == null) { isList = seq; }
            else if (isList != seq) { break; }

            if (seq)
            {
                node.List ??= new List<YamlNode>();
                string rest = text.Length > 1 ? text.Substring(2) : string.Empty;
                if (rest.Length == 0)
                {
                    i++;
                    int deeper = NextIndent(lines, i, indent);
                    node.List.Add(deeper > indent ? ParseBlock(lines, ref i, deeper) : new YamlNode());
                }
                else
                {
                    // เนื้อหาเริ่มบนบรรทัดเดียวกับขีด — เขียนบรรทัดใหม่ให้เป็นบล็อกย่อยปกติ
                    // (แก้ในอาเรย์ที่โหลดมาเฉพาะรอบนี้ ไม่แตะไฟล์บนดิสก์)
                    lines[i] = new string(' ', indent + 2) + rest;
                    node.List.Add(ParseBlock(lines, ref i, indent + 2));
                }
                continue;
            }

            int colon = FindKeyColon(text);
            if (colon < 0)
            {
                // ไม่มี ':' = เป็นค่าเดี่ยว ๆ เช่นตัวเลขใต้ขีดของลิสต์ (`- 81`)
                // ถ้าบล็อกนี้ยังไม่มีอะไรเลย แปลว่าทั้งบล็อกคือค่าเดี่ยวตัวนี้
                if (node.Map == null && node.List == null)
                {
                    i++;
                    return new YamlNode { Scalar = text.Trim().Trim('\'', '"') };
                }
                i++;
                continue;                              // บรรทัดที่ไม่เข้าโครง — ข้าม
            }

            string key = text.Substring(0, colon).Trim().Trim('\'', '"');
            string value = text.Substring(colon + 1).Trim();
            node.Map ??= new Dictionary<string, YamlNode>(StringComparer.Ordinal);
            i++;

            if (value == "{}" || value == "[]")
            {
                node.Map[key] = new YamlNode();        // ว่างเปล่า
                continue;
            }
            if (value.Length > 0)
            {
                node.Map[key] = new YamlNode { Scalar = value.Trim('\'', '"') };
                continue;
            }

            int childIndent = NextIndent(lines, i, -1);
            if (childIndent > indent)
            {
                node.Map[key] = ParseBlock(lines, ref i, childIndent);
            }
            else if (childIndent == indent && IsSequenceAt(lines, i, indent))
            {
                // YAML ยอมให้ "- " ของลิสต์อยู่ย่อหน้าเดียวกับคีย์ที่เป็นเจ้าของ
                node.Map[key] = ParseBlock(lines, ref i, indent);
            }
            else
            {
                node.Map[key] = new YamlNode();
            }
        }
        return node;
    }

    /// <summary>ตำแหน่ง ':' ที่ทำหน้าที่เป็นตัวคั่นคีย์ (ข้ามที่อยู่ในเครื่องหมายคำพูด)</summary>
    private static int FindKeyColon(string text)
    {
        bool quoted = false;
        char quote = '\0';
        for (int i = 0; i < text.Length; i++)
        {
            char c = text[i];
            if (quoted)
            {
                if (c == quote) { quoted = false; }
                continue;
            }
            if (c == '\'' || c == '"') { quoted = true; quote = c; continue; }
            if (c == ':' && (i + 1 >= text.Length || text[i + 1] == ' ')) { return i; }
        }
        return -1;
    }

    private static int NextIndent(string[] lines, int i, int fallback)
    {
        while (i < lines.Length && IsSkippable(lines[i])) { i++; }
        return i < lines.Length ? IndentOf(lines[i]) : fallback;
    }

    private static bool IsSequenceAt(string[] lines, int i, int indent)
    {
        while (i < lines.Length && IsSkippable(lines[i])) { i++; }
        if (i >= lines.Length) { return false; }
        string t = lines[i].TrimStart();
        return IndentOf(lines[i]) >= indent && (t.StartsWith("- ", StringComparison.Ordinal) || t == "-");
    }
}
