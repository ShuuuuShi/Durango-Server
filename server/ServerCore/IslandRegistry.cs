using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace DurangoServer.Core;

/// <summary>
/// Beta 1.1 — **เกาะแยกตามช่วงเลเวล**
///
/// 1 เกาะ = 1 process (คนละ terrain · คนละ world save · คนละ config · คนละพอร์ต)
/// แต่ **ตัวละครใช้ไฟล์เซฟร่วมกัน** (`saves/players/`) เพราะเดินทางข้ามเกาะแล้วต้องเอาของกับเลเวลไปด้วย
///
/// ทำไมต้องแยก process ไม่ใช่หลายโลกใน process เดียว:
/// ตัวเกมโหลด terrain ครั้งเดียวตอนเข้าเกม (`/terrains/1` ของ gateway) แล้วสตรีมทีละก้อนจากอันนั้น
/// จะเปลี่ยนเกาะทั้งที client ต้องตัดการเชื่อมต่อแล้วเข้าใหม่อยู่ดี — แยก process จึงตรงกับที่ client ทำอยู่แล้ว
///
/// ไฟล์ทะเบียน: `data/islands.json` (ทุกเกาะอ่านไฟล์เดียวกัน จะได้รู้จักกันเอง)
/// </summary>
public static class IslandRegistry
{
    /// <summary>เกาะที่ server ตัวนี้เป็นอยู่ (null = โหมดเกาะเดียวแบบเดิม)</summary>
    public static IslandInfo Current { get; private set; }

    private static List<IslandInfo> _all = new List<IslandInfo>();
    private static string _path;

    public static IReadOnlyList<IslandInfo> All => _all;

    /// <summary>โหลดทะเบียน (ไม่มีไฟล์ = สร้างให้พร้อมเกาะตัวอย่าง 2 เกาะ)</summary>
    public static void Load(string path)
    {
        _path = path;
        try
        {
            if (!File.Exists(path))
            {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
                File.WriteAllText(path, JsonConvert.SerializeObject(new IslandFile { Islands = IslandInfo.Defaults() }, Formatting.Indented));
                Console.WriteLine("[island] ไม่มี {0} — สร้างทะเบียนเกาะเริ่มต้นให้แล้ว", path);
            }
            IslandFile file = JsonConvert.DeserializeObject<IslandFile>(File.ReadAllText(path));
            _all = file?.Islands ?? IslandInfo.Defaults();
        }
        catch (Exception e)
        {
            Console.WriteLine("[island] อ่าน {0} ไม่ได้ ({1}) — ใช้ทะเบียนเริ่มต้น", path, e.Message);
            _all = IslandInfo.Defaults();
        }
    }

    /// <summary>เลือกว่า server ตัวนี้เป็นเกาะไหน คืน false ถ้าไม่มี id นั้นในทะเบียน</summary>
    public static bool Select(string islandId)
    {
        for (int i = 0; i < _all.Count; i++)
        {
            if (string.Equals(_all[i].Id, islandId, StringComparison.OrdinalIgnoreCase))
            {
                Current = _all[i];
                return true;
            }
        }
        Console.WriteLine("[island] ไม่มีเกาะ '{0}' ในทะเบียน — มีอยู่: {1}", islandId, string.Join(", ", Ids()));
        return false;
    }

    public static IslandInfo Find(string islandId)
    {
        for (int i = 0; i < _all.Count; i++)
        {
            if (string.Equals(_all[i].Id, islandId, StringComparison.OrdinalIgnoreCase))
            {
                return _all[i];
            }
        }
        return null;
    }

    public static string[] Ids()
    {
        var ids = new string[_all.Count];
        for (int i = 0; i < _all.Count; i++)
        {
            ids[i] = _all[i].Id;
        }
        return ids;
    }

    /// <summary>เกาะที่ผู้เล่นเลเวลนี้ไปได้ (ไว้ตอบตอนถามว่าจะเดินทางไปไหนได้บ้าง)</summary>
    public static List<IslandInfo> ReachableFor(int playerLevel)
    {
        var list = new List<IslandInfo>();
        for (int i = 0; i < _all.Count; i++)
        {
            if (playerLevel >= _all[i].RequiredLevel)
            {
                list.Add(_all[i]);
            }
        }
        return list;
    }

    private sealed class IslandFile
    {
        public List<IslandInfo> Islands { get; set; }
    }
}

public sealed class IslandInfo
{
    /// <summary>รหัสสั้น ๆ ใช้เป็นชื่อไฟล์เซฟและพารามิเตอร์ `--island`</summary>
    public string Id { get; set; }
    public string Name { get; set; }
    /// <summary>ชื่อไฟล์ terrain ใน data/terrains (ไม่ต้องใส่ .zip)</summary>
    public string Terrain { get; set; }
    /// <summary>ช่วงเลเวลของสัตว์บนเกาะนี้</summary>
    public int MinLevel { get; set; }
    public int MaxLevel { get; set; }
    /// <summary>ผู้เล่นต้องเลเวลเท่านี้ขึ้นไปถึงจะเดินทางมาได้</summary>
    public int RequiredLevel { get; set; }
    public string Host { get; set; }
    public int GatewayPort { get; set; }
    public int GamePort { get; set; }

    /// <summary>ที่อยู่ที่ client ใช้ต่อ (ตัวเกมต่อ gateway ก่อนเสมอ)</summary>
    public string Address => $"{Host}:{GatewayPort}";

    public override string ToString()
    {
        return $"{Id} ({Name}) lv{MinLevel}-{MaxLevel} ต้องเลเวล {RequiredLevel}+ @ {Address}";
    }

    /// <summary>
    /// ทะเบียนเริ่มต้น 3 เกาะ — terrain ทั้งหมดมีอยู่ใน data/terrains แล้ว
    /// ตัวเลขในชื่อ terrain ของเกมคือช่วงเลเวลของมันเอง (pe10gr = เกาะส่วนตัว lv10,
    /// ri35te = เกาะปกติ lv35) แต่ **เลเวลสัตว์จริงมาจาก config ของเราไม่ใช่ชื่อไฟล์**
    /// จึงเอา terrain ไหนมาทำเกาะเลเวลอะไรก็ได้ — เลือกตามหน้าตาภูมิประเทศเป็นหลัก
    /// </summary>
    public static List<IslandInfo> Defaults()
    {
        return new List<IslandInfo>
        {
            new IslandInfo
            {
                Id = "isle01", Name = "เกาะเริ่มต้น", Terrain = "ri35te",
                MinLevel = 1, MaxLevel = 10, RequiredLevel = 1,
                Host = "127.0.0.1", GatewayPort = 8190, GamePort = 8191
            },
            new IslandInfo
            {
                Id = "isle02", Name = "เกาะป่าลึก", Terrain = "ri40tr",
                MinLevel = 10, MaxLevel = 20, RequiredLevel = 8,
                Host = "127.0.0.1", GatewayPort = 8290, GamePort = 8291
            },
            new IslandInfo
            {
                Id = "isle03", Name = "เกาะทะเลทราย", Terrain = "ri35de",
                MinLevel = 20, MaxLevel = 30, RequiredLevel = 18,
                // ⚠️ ห้ามใช้ 8390/8391 — ตัวเกมมี server ของตัวเองอยู่บนสองพอร์ตนั้น
                Host = "127.0.0.1", GatewayPort = 8490, GamePort = 8491
            },
        };
    }
}
