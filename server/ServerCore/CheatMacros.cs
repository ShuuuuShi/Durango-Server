using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Newtonsoft.Json.Linq;

namespace DurangoServer.Core;

// ============================================================================
// CheatMacros — มาโครคำสั่งทดสอบที่ "ตัวเกมนิยามมาเอง"
//
// ไฟล์ต้นทาง: Resources/cheat_macro_definition.json ที่ AssetRipper ถอดออกมาจากตัวเกม
// ก๊อปมาไว้ที่ data/cheat_macros.json (เซิร์ฟอ่านจากที่นั่น จะได้แก้เพิ่มเองได้)
//
// เป็นชุดคำสั่งที่ทีมสร้างเกมใช้เซ็ตตัวละครสำหรับเทสจริง ๆ เช่น
//   makecharacter_leave_ancora/mcla = set level 5 · sc survival 5 · it axe_... · ga · la
// คีย์เขียนแบบ "ชื่อเต็ม/ชื่อย่อ" เรียกด้วยชื่อไหนก็ได้
// ============================================================================

public static class CheatMacros
{
    private static Dictionary<string, string[]> _macros;
    private static Dictionary<string, string> _alias;

    /// <summary>พาธไฟล์ที่โหลดสำเร็จ (null = ยังไม่เจอไฟล์)</summary>
    public static string SourcePath { get; private set; }

    public static void Load(string dataDir)
    {
        _macros = new Dictionary<string, string[]>(StringComparer.OrdinalIgnoreCase);
        _alias = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        SourcePath = null;

        string path = Path.Combine(dataDir ?? "data", "cheat_macros.json");
        if (!File.Exists(path))
        {
            return;
        }
        try
        {
            JObject root = JObject.Parse(File.ReadAllText(path));
            foreach (KeyValuePair<string, JToken> kv in root)
            {
                if (kv.Value is not JArray arr)
                {
                    continue;
                }
                string[] lines = arr.Select(x => x.ToString()).Where(s => s.Length > 0).ToArray();
                if (lines.Length == 0)
                {
                    continue;
                }
                _macros[kv.Key] = lines;
                // คีย์เขียนเป็น "ชื่อเต็ม/ชื่อย่อ" — ลงทะเบียนทุกชื่อให้เรียกได้หมด
                foreach (string part in kv.Key.Split('/', StringSplitOptions.RemoveEmptyEntries))
                {
                    _alias[part.Trim()] = kv.Key;
                }
            }
            SourcePath = path;
            Console.WriteLine("[cheat] โหลดมาโครของเกม {0} ชุด จาก {1}", _macros.Count, Path.GetFileName(path));
        }
        catch (Exception e)
        {
            Console.WriteLine("[cheat] อ่าน cheat_macros.json ไม่ได้: {0}", e.Message);
        }
    }

    public static IEnumerable<KeyValuePair<string, string[]>> All()
    {
        return _macros ?? new Dictionary<string, string[]>();
    }

    /// <summary>คำสั่งของมาโครนี้ — รับทั้งชื่อเต็ม ชื่อย่อ และคีย์เต็มที่มีขีดคั่น</summary>
    public static string[] Commands(string name)
    {
        if (_macros == null || string.IsNullOrWhiteSpace(name))
        {
            return null;
        }
        string key = name.Trim();
        if (_macros.TryGetValue(key, out string[] direct))
        {
            return direct;
        }
        return _alias != null && _alias.TryGetValue(key, out string full) ? _macros[full] : null;
    }
}
