using System;

namespace DurangoServer.Core;

/// <summary>
/// ตารางค่าประสบการณ์ต่อเลเวล — **ค่าจริงของเกม**
///
/// สกัดอัตโนมัติจาก game/DurangoV2_Data/resources.strings.txt (`level_thresholds`)
/// ด้วย scripts/extract_levels.py — **อย่าแก้ด้วยมือ**
///
/// ตัวเลขคือ exp **สะสม** ที่ต้องถึงเพื่อขึ้นเลเวลถัดไป (ค่าแรก = exp ที่ต้องมีเพื่อเป็น lv2)
/// client ใช้ตารางชุดเดียวกันนี้วาดหลอด exp จึงต้องตรงกันเป๊ะ ไม่งั้นหลอดกับเลเวลไม่ตรงกัน
/// </summary>
public static class LevelData
{
    /// <summary>เพดานสูงสุดที่โค้ดรองรับ (ตารางมีถึง 81) — ใช้ตรวจค่าที่ client อ้างมา</summary>
    public const int MaxLevel = 60;

    /// <summary>
    /// เพดานของ **รอบนี้** — ปรับได้ที่ `Features.MaxPlayerLevel` ใน config.json
    /// 0 = ไม่จำกัด (ใช้ MaxLevel = 60 ตามเกม) — ค่าเริ่มต้นตั้งแต่ 3 ก.ย. 2026
    /// (beta 1.0.0 เคยล็อก 20 ตาม LBT1 ของเกมต้นฉบับ — ดู TodoList/01-level-cap.md)
    /// </summary>
    public static int Cap
    {
        get
        {
            int c = ServerConfig.Current.Features.MaxPlayerLevel;
            return c > 0 && c < MaxLevel ? c : MaxLevel;
        }
    }

    private static readonly int[] Thresholds =
    {
        11, 25, 44, 70, 107, 158, 225, 304, 402, 524,
        733, 1006, 1351, 1784, 2324, 2991, 3811, 4812, 6028, 7498,
        9267, 11387, 13918, 16930, 20502, 24726, 29708, 35568, 42445, 50497,
        59905, 70875, 83644, 98479, 115687, 135616, 158662, 185275, 215965, 251312,
        291974, 338695, 392320, 453803, 524225, 604808, 696933, 802160, 922250, 1059190,
        1215222, 1392874, 1594995, 1824793, 2085883, 2382332, 2718718, 3100190, 3532536, 4022264,
        4576686, 5204015, 5913473, 6715414, 7621456, 8644633, 9799564, 11102640, 12572235, 14228939,
        16095820, 18198715, 20566556, 23231733, 26230497, 29603411, 33395855, 37658585, 42448356, 47828620,
    };

    /// <summary>exp สะสมที่ต้องมีเพื่อเป็นเลเวลนี้ (เลเวล 1 = 0)</summary>
    public static int RequiredFor(int level)
    {
        if (level <= 1)
        {
            return 0;
        }
        int idx = Math.Min(level - 2, Thresholds.Length - 1);
        return Thresholds[idx];
    }

    /// <summary>เลเวลที่ควรเป็นเมื่อมี exp สะสมเท่านี้</summary>
    public static int LevelFor(int totalExp)
    {
        int level = 1;
        int cap = Cap;
        while (level < cap && totalExp >= RequiredFor(level + 1))
        {
            level++;
        }
        return level;
    }

    /// <summary>exp ที่ยังต้องเก็บอีกเพื่อขึ้นเลเวลถัดไป (เต็มเพดานแล้วคืน 0)</summary>
    public static int ToNextLevel(int totalExp)
    {
        int level = LevelFor(totalExp);
        if (level >= Cap)
        {
            return 0;
        }
        return RequiredFor(level + 1) - totalExp;
    }
}
