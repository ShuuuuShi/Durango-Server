using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DurangoServer.Core;

/// <summary>
/// เครื่องมือที่ต้องใช้ตอนเก็บของจากธรรมชาติ — อ่านจาก <c>data/gathering_tools.json</c>
/// ไม่ hardcode ใน Gathering.cs — โหลดตอนเปิดเซิร์ฟ แก้ไฟล์แล้วพิมพ์ `cheat reload gather`
///
/// ไม่มีในตาราง = มือเปล่าเก็บได้
/// ค่าเป็น tag ของไอเทม (axe / knife / pickaxe) ไม่ใช่ชื่อไอเทม
/// </summary>
public static class GatheringTools
{
    private static readonly object Lock = new object();
    private static Dictionary<string, string> _tools = new Dictionary<string, string>(StringComparer.Ordinal);
    private static string _path;
    private static DateTime _lastWrite;

    public static string FilePath => _path;

    private sealed class FileRoot
    {
        public Dictionary<string, string> Tools { get; set; }
    }

    public static void Load(string dataDir)
    {
        _path = Path.Combine(dataDir ?? "data", "gathering_tools.json");
        if (!File.Exists(_path))
        {
            var seed = new FileRoot
            {
                Tools = new Dictionary<string, string>(StringComparer.Ordinal)
                {
                    ["wood_log"] = "axe",
                    ["wood_bough"] = "knife",   // 3 ก.ย. 2026 เจ้าของสั่ง: ทำขวานต้องใช้กิ่งไม้ → กิ่งไม้ต้องเก็บด้วยมีด ไม่งั้นวนตาย
                    ["wood_bush"] = "knife",
                    ["stone_big"] = "pickaxe",
                    ["metal_brass"] = "pickaxe"
                }
            };
            try
            {
                Directory.CreateDirectory(Path.GetDirectoryName(_path)!);
                File.WriteAllText(_path, JsonConvert.SerializeObject(seed, Formatting.Indented));
                Console.WriteLine("[gather] ไม่มี {0} — สร้างไฟล์ค่าเริ่มต้นให้แล้ว", _path);
            }
            catch (Exception e)
            {
                Console.WriteLine("[gather] เขียน {0} ไม่ได้: {1}", _path, e.Message);
            }
        }
        Reload(quiet: false);
    }

    /// <summary>โหลดไฟล์ใหม่ตามคำสั่ง — ไม่ได้วนเช็คเอง</summary>
    public static string ReloadNow()
    {
        if (string.IsNullOrEmpty(_path))
        {
            return "ยังไม่ได้โหลด gathering_tools.json";
        }
        if (!File.Exists(_path))
        {
            return "ไม่เจอไฟล์ " + _path;
        }
        int n = Reload(quiet: false);
        return n < 0 ? "อ่านไฟล์ไม่สำเร็จ ดู log" : $"โหลดเครื่องมือเก็บของ {n} รายการจาก {_path}";
    }

    /// <summary>tag เครื่องมือที่ต้องใช้ หรือ null ถ้ามือเปล่า</summary>
    public static string RequiredTag(string prototype)
    {
        if (string.IsNullOrEmpty(prototype))
        {
            return null;
        }
        lock (Lock)
        {
            if (_tools.TryGetValue(prototype, out string tag))
            {
                return string.IsNullOrWhiteSpace(tag) ? null : tag;
            }
            foreach (KeyValuePair<string, string> pair in _tools)
            {
                if (prototype.StartsWith(pair.Key, StringComparison.Ordinal)
                    && (prototype.Length == pair.Key.Length || prototype[pair.Key.Length] == '_'))
                {
                    return string.IsNullOrWhiteSpace(pair.Value) ? null : pair.Value;
                }
            }
        }
        return null;
    }

    private static int Reload(bool quiet)
    {
        try
        {
            FileRoot loaded = JsonConvert.DeserializeObject<FileRoot>(File.ReadAllText(_path)) ?? new FileRoot();
            Dictionary<string, string> map = new Dictionary<string, string>(StringComparer.Ordinal);
            if (loaded.Tools != null)
            {
                foreach (KeyValuePair<string, string> pair in loaded.Tools)
                {
                    if (!string.IsNullOrWhiteSpace(pair.Key))
                    {
                        map[pair.Key.Trim()] = (pair.Value ?? "").Trim();
                    }
                }
            }
            lock (Lock)
            {
                _tools = map;
                _lastWrite = File.GetLastWriteTimeUtc(_path);
            }
            if (!quiet)
            {
                Console.WriteLine("[gather] โหลดเครื่องมือเก็บของ {0} รายการจาก {1}", map.Count, _path);
            }
            return map.Count;
        }
        catch (Exception e)
        {
            Console.WriteLine("[gather] อ่าน {0} ไม่ได้: {1} — ใช้ตารางเดิมต่อ", _path, e.Message);
            return -1;
        }
    }
}
