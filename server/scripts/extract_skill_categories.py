"""สกัดตารางความชำนาญของหมวดสกิล (TextAsset `categories`) -> SkillCategoryData.cs

ในเกมจริง "หมวดสกิล" (ล่า/เก็บของ/ทำอาหาร/คราฟต์/ก่อสร้าง) มี **เลเวลของตัวเองที่ขึ้นเองจากการทำงาน**
คนละอย่างกับสกิลย่อยที่ต้องใช้แต้มไปกดเรียน — server เราทำแต่ครึ่งหลัง หมวดเลยค้างที่ 0 ตลอด

ตารางในไฟล์นี้คือ **exp ที่ต้องใช้ต่อ 1 เลเวลของแต่ละหมวด** ตามข้อมูลจริงของเกม
ตัวเลขเล็กมาก (เลเวล 1->2 ใช้ 1 · เลเวล 40->41 ใช้ 27) ⇒ สเกลคือ "จำนวนครั้งที่ทำสำเร็จ" ไม่ใช่แต้มใหญ่ ๆ

key ของ asset = เลข enum Shared.Skill.Category (0 = Survival, 7 = Gathering, 8 = Cooking, ...)

ใช้: python scripts/extract_skill_categories.py <resources.strings.txt> <ServerCore/SkillCategoryData.cs>
"""
import io
import json
import sys
import pathlib

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])

lines = io.open(SRC, 'r', encoding='utf-8', errors='replace').read().split('\n')


def load_asset(name):
    start = None
    for i, line in enumerate(lines):
        if line.rstrip() == name:
            start = i + 1
            break
    if start is None:
        raise SystemExit('หา TextAsset `%s` ใน dump ไม่เจอ' % name)
    body = []
    for line in lines[start:]:
        if line and not line[0].isspace():
            break
        body.append(line)
    blob = '\n'.join(body).rstrip().rstrip(',')
    last = None
    for extra in range(1, 6):
        try:
            return json.loads('{' + blob + '}' * extra)
        except Exception as e:      # noqa: BLE001
            last = e
    raise SystemExit('parse JSON ของ %s ไม่ได้: %s' % (name, last))


categories = load_asset('categories')

rows = []
for key in sorted(categories, key=lambda k: int(k)):
    entry = categories[key]
    needed = entry.get('exp_needed') or {}
    level = entry.get('level') or {}
    lo = int(level.get('min', 1))
    hi = int(level.get('max', 60))
    # เก็บเป็น array เรียงตามเลเวล 1..hi-1 (ช่องที่ i = exp ที่ต้องใช้เพื่อขึ้นจากเลเวล i+1)
    values = []
    for lv in range(1, hi):
        values.append(int(needed.get(str(lv), 0)))
    rows.append('        { %s, new Curve(%d, %d, new[] { %s }) },' % (
        key, lo, hi, ', '.join(str(v) for v in values)))

src = '''using System;
using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// ตารางความชำนาญของหมวดสกิล — **สร้างอัตโนมัติ อย่าแก้ด้วยมือ**
/// (`python scripts/extract_skill_categories.py "../game/DurangoV2_Data/resources.strings.txt" ServerCore/SkillCategoryData.cs`)
///
/// มาจาก TextAsset `categories` — %d หมวด
///
/// ในเกมมีสกิล 2 ชั้น:
///   1. **สกิลย่อย** — ใช้แต้มจากการขึ้นเลเวลไปกดเรียนเอง (ServerPlayer.Skills)
///   2. **ความชำนาญของหมวด** — ขึ้นเองจากการทำงานซ้ำ ๆ (ไฟล์นี้ + ServerPlayer.Proficiency)
///
/// ⚠️ ตัวเลข exp เล็กมาก (เลเวล 1→2 ใช้ 1 · 40→41 ใช้ 27) เพราะสเกลของเกมคือ
/// **"จำนวนครั้งที่ทำสำเร็จ"** ไม่ใช่แต้มก้อนใหญ่ — ทำสำเร็จ 1 ครั้ง = 1 exp
///
/// client ใช้ตารางเดียวกันนี้วาดหลอดความคืบหน้า (Durango.Logic.Skill.Category.IsReadyToResearch)
/// ⇒ **ค่า Exp ที่ส่งไปต้องเป็น "exp ที่สะสมในเลเวลปัจจุบัน" ไม่ใช่ exp รวมทั้งหมด**
/// </summary>
public static class SkillCategoryData
{
    public sealed class Curve
    {
        public readonly int MinLevel;
        public readonly int MaxLevel;
        /// <summary>ช่อง i = exp ที่ต้องใช้เพื่อขึ้นจากเลเวล (i+1) ไป (i+2)</summary>
        public readonly int[] ExpNeeded;

        public Curve(int minLevel, int maxLevel, int[] expNeeded)
        {
            MinLevel = minLevel;
            MaxLevel = maxLevel;
            ExpNeeded = expNeeded;
        }

        /// <summary>exp ที่ต้องใช้เพื่อขึ้นจากเลเวลนี้ไปเลเวลถัดไป (0 = ขึ้นต่อไม่ได้แล้ว)</summary>
        public int NeededAt(int level)
        {
            int index = level - 1;
            if (index < 0 || index >= ExpNeeded.Length)
            {
                return 0;
            }
            return ExpNeeded[index];
        }
    }

    /// <summary>เลข enum Shared.Skill.Category -> ตารางของหมวดนั้น</summary>
    public static readonly Dictionary<int, Curve> Map = new Dictionary<int, Curve>()
    {
%s
    };

    public static bool TryGet(Shared.Skill.Category category, out Curve curve)
    {
        return Map.TryGetValue((int)category, out curve);
    }

    /// <summary>
    /// แปลง exp รวมของหมวดหนึ่ง -> (เลเวล, exp ที่เหลือในเลเวลนั้น)
    ///
    /// เก็บ "exp รวม" ในไฟล์เซฟแล้วคิดเลเวลใหม่ทุกครั้ง (แบบเดียวกับเลเวลผู้เล่นที่คิดจาก exp)
    /// ⇒ ปรับตารางวันหลังแล้วเซฟเก่าไม่เพี้ยน
    /// </summary>
    public static void Resolve(Shared.Skill.Category category, int totalExp, int levelCap, out int level, out int expInLevel)
    {
        level = 1;
        expInLevel = 0;
        if (!TryGet(category, out Curve curve))
        {
            return;
        }
        level = curve.MinLevel;
        int remaining = Math.Max(0, totalExp);
        int max = curve.MaxLevel;
        if (levelCap > 0 && levelCap < max)
        {
            max = levelCap;
        }
        while (level < max)
        {
            int needed = curve.NeededAt(level);
            if (needed <= 0 || remaining < needed)
            {
                break;
            }
            remaining -= needed;
            level++;
        }
        expInLevel = remaining;
    }
}
''' % (len(categories), '\n'.join(rows))

OUT.write_text(src, encoding='utf-8')
print('เขียน %s แล้ว — %d หมวด' % (OUT, len(categories)))
