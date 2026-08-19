"""สกัดตารางค่าประสบการณ์ต่อเลเวลจาก resources.strings.txt → LevelData.cs

ตาราง `level_thresholds` คือ **ค่าประสบการณ์สะสม** ที่ต้องถึงเพื่อขึ้นเลเวลถัดไป
(ค่าแรก 11 = exp ที่ต้องมีเพื่อเป็นเลเวล 2)

ใช้: python scripts/extract_levels.py "../game/DurangoV2_Data/resources.strings.txt" ServerCore/LevelData.cs
"""
import json, re, sys, pathlib

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])
lines = SRC.read_text(encoding='utf-8', errors='replace').split('\n')

thresholds = None
for i, ln in enumerate(lines):
    if ln.strip().startswith('"level_thresholds"'):
        # อ่านตัวเลขไปจนเจอ ']'
        nums = []
        j = i + 1
        while j < len(lines) and ']' not in lines[j]:
            t = lines[j].strip().rstrip(',')
            if t:
                nums.append(int(t))
            j += 1
        # ตารางของ player จะยาวกว่าตารางอื่น ๆ — เอาอันที่ยาวที่สุด
        if thresholds is None or len(nums) > len(thresholds):
            thresholds = nums

if not thresholds:
    print('หา level_thresholds ไม่เจอ')
    sys.exit(1)

print('เจอ %d ระดับ (เลเวล 2-%d)' % (len(thresholds), len(thresholds) + 1))

rows = []
for i in range(0, len(thresholds), 10):
    rows.append('        ' + ', '.join(str(n) for n in thresholds[i:i + 10]) + ',')

src = '''using System;

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
    /// <summary>เพดานเลเวลของ beta (ตารางมีถึง %d)</summary>
    public const int MaxLevel = 60;

    private static readonly int[] Thresholds =
    {
%s
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
        while (level < MaxLevel && totalExp >= RequiredFor(level + 1))
        {
            level++;
        }
        return level;
    }

    /// <summary>exp ที่ยังต้องเก็บอีกเพื่อขึ้นเลเวลถัดไป (เต็มเพดานแล้วคืน 0)</summary>
    public static int ToNextLevel(int totalExp)
    {
        int level = LevelFor(totalExp);
        if (level >= MaxLevel)
        {
            return 0;
        }
        return RequiredFor(level + 1) - totalExp;
    }
}
''' % (len(thresholds) + 1, '\n'.join(rows))

OUT.write_text(src, encoding='utf-8')
print('เขียน %s แล้ว' % OUT)
