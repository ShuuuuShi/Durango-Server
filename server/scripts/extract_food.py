"""สกัดข้อมูลอาหาร (TextAsset `performance` หัวข้อ food) จาก resources.strings.txt -> FoodData.cs

ทำไมต้องมี: เดิม server ตัดสินว่า "กินได้ไหม" จาก **คำที่อยู่ในชื่อ prototype**
(meat/fruit/egg/...) แล้วเติมสตามินาเท่ากันหมด 30 หน่วยทุกอย่าง
⇒ ต้มน้ำซุปทั้งวันก็ได้ผลเท่ากับกัดผลไม้ดิบ · ของที่กินไม่ได้บางอย่างก็กินได้

ข้อมูลจริงของเกมมีครบ 352 ชนิด แต่ละชนิดบอก:
  · energy_potential  — สตามินาที่ได้ (เป็นสูตรของเลเวลไอเทม เช่น "18 + 0.50 * (level -1)")
  · health / life     — เลือดที่ได้
  · fatigue           — ความล้าที่ลด (ค่าติดลบในข้อมูลเกม)
  · satiety           — ความอิ่ม (ยังไม่มีหลอดนี้ใน server — เก็บไว้ให้ระบบความหิวรอบหน้า)
  · water             — ดับกระหายไหม
  · eat_motion        — ท่ากิน (Eat / Drink) ที่ client เล่น
  · effect_on / modifier_effect_time — บัฟหลังกิน
  · digestivetime     — กี่วินาทีถึงกินชิ้นถัดไปได้

สูตรทุกอันเป็นเส้นตรงกับ level จึงเก็บเป็น (ค่าที่เลเวล 1, ค่าที่เพิ่มต่อเลเวล)
โดยคำนวณจาก f(1) กับ f(2) ตอนสกัด

ใช้: python scripts/extract_food.py <resources.strings.txt> <ServerCore/FoodData.cs>
"""
import io
import json
import re
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


performance = load_asset('performance')
foods = performance.get('food') or {}

SAFE = re.compile(r'^[0-9levl\s\.\+\-\*/\(\)]*$')


def value_at(expr, level):
    """คิดค่าของสูตรที่เลเวลหนึ่ง — สูตรในข้อมูลเกมมีแค่บวก/ลบ/คูณกับ level"""
    if expr is None:
        return 0.0
    s = str(expr).strip()
    if s == '':
        return 0.0
    try:
        return float(s)
    except ValueError:
        pass
    if not SAFE.match(s):
        return 0.0
    try:
        return float(eval(s, {'__builtins__': {}}, {'level': level}))    # noqa: S307 — สูตรถูกกรองด้วย SAFE แล้ว
    except Exception:               # noqa: BLE001
        return 0.0


def linear(expr):
    """คืน (ค่าที่เลเวล 1, ค่าที่เพิ่มต่อ 1 เลเวล)"""
    a = value_at(expr, 1)
    b = value_at(expr, 2)
    return a, b - a


def rng(key):
    """"[1, 70]" -> (1, 70)"""
    nums = re.findall(r'-?\d+', key)
    if len(nums) >= 2:
        return int(nums[0]), int(nums[1])
    return 1, 70


def cs_str(value):
    if value is None or value == '':
        return 'null'
    return '"%s"' % str(value).replace('\\', '\\\\').replace('"', '\\"')


def g(x):
    return '%g' % round(x, 4)


rows = []
count = 0
for proto in sorted(foods):
    entries = []
    for key, v in sorted(foods[proto].items(), key=lambda kv: rng(kv[0])[0]):
        lo, hi = rng(key)
        e_base, e_step = linear(v.get('energy_potential'))
        h_base, h_step = linear(v.get('health'))
        l_base, l_step = linear(v.get('life'))
        entries.append('E(%d,%d,%sf,%sf,%sf,%sf,%sf,%sf,%sf,%sf,%sf,%d,%s,%s,%sf)' % (
            lo, hi,
            g(e_base), g(e_step),
            g(h_base), g(h_step),
            g(l_base), g(l_step),
            g(value_at(v.get('satiety'), 1)),
            g(value_at(v.get('fatigue'), 1)),
            g(value_at(v.get('water'), 1)),
            int(value_at(v.get('digestivetime'), 1)),
            cs_str(v.get('eat_motion')),
            cs_str(v.get('effect_on')),
            g(value_at(v.get('modifier_effect_time'), 1)),
        ))
        count += 1
    rows.append('        { %s, new[] { %s } },' % (cs_str(proto), ', '.join(entries)))

src = '''using System;
using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// ข้อมูลอาหารของเกม — **สร้างอัตโนมัติ อย่าแก้ด้วยมือ**
/// (`python scripts/extract_food.py "../game/DurangoV2_Data/resources.strings.txt" ServerCore/FoodData.cs`)
///
/// มาจาก TextAsset `performance` หัวข้อ <c>food</c> — ของกินได้ %d ชนิด
/// แทนที่การเดาจากชื่อ prototype แบบเดิม (มีคำว่า meat/fruit/egg = กินได้ +30 สตามินาเท่ากันหมด)
///
/// ค่าที่ขึ้นกับเลเวลของไอเทมเก็บเป็น (ค่าที่เลเวล 1, ค่าที่เพิ่มต่อเลเวล) เพราะสูตรในข้อมูลเกม
/// เป็นเส้นตรงทั้งหมด เช่น "18 + 0.50 * (level -1)"
///
/// ⚠️ ตัวเลขเป็น **สเกลของเกมต้นฉบับ** (ความล้า -150, สตามินา 18-40)
/// ส่วน server เราใช้หลอด 0-100 ⇒ ต้องคูณตัวคูณใน <c>data/config.json</c> หัวข้อ Food ก่อนใช้จริง
/// ดู ServerPlayer.Items.HandleUseItem
/// </summary>
public static class FoodData
{
    /// <summary>ข้อมูลอาหารช่วงเลเวลหนึ่ง (ของบางอย่างมีค่าต่างกันระหว่างเลเวล 1-14 กับ 15-70)</summary>
    public sealed class Entry
    {
        public readonly int MinLevel;
        public readonly int MaxLevel;
        /// <summary>สตามินาที่ได้ที่เลเวล 1 · และที่เพิ่มต่อเลเวล</summary>
        public readonly float EnergyBase;
        public readonly float EnergyPerLevel;
        /// <summary>เลือดที่ได้ (health = ฟื้นทันที)</summary>
        public readonly float HealthBase;
        public readonly float HealthPerLevel;
        /// <summary>เลือดสูงสุดที่เพิ่ม (life) — ในข้อมูลเกมส่วนใหญ่เป็น 0</summary>
        public readonly float LifeBase;
        public readonly float LifePerLevel;
        /// <summary>ความอิ่ม — ยังไม่มีหลอดนี้ใน server (เก็บไว้ให้ระบบความหิว)</summary>
        public readonly float Satiety;
        /// <summary>ความล้าที่เปลี่ยน — **ติดลบ = ลดความล้า**</summary>
        public readonly float Fatigue;
        /// <summary>ดับกระหายไหม (0/1)</summary>
        public readonly float Water;
        /// <summary>กินแล้วต้องรอกี่วินาทีถึงกินชิ้นถัดไปได้</summary>
        public readonly int DigestiveTime;
        /// <summary>ท่าที่ client เล่นตอนกิน (Eat / Drink)</summary>
        public readonly string EatMotion;
        /// <summary>บัฟที่ติดหลังกิน (ยังไม่ได้ใช้ — รอระบบ status effect)</summary>
        public readonly string EffectOn;
        public readonly float EffectSeconds;

        public Entry(int minLevel, int maxLevel, float energyBase, float energyPerLevel,
            float healthBase, float healthPerLevel, float lifeBase, float lifePerLevel,
            float satiety, float fatigue, float water, int digestiveTime,
            string eatMotion, string effectOn, float effectSeconds)
        {
            MinLevel = minLevel; MaxLevel = maxLevel;
            EnergyBase = energyBase; EnergyPerLevel = energyPerLevel;
            HealthBase = healthBase; HealthPerLevel = healthPerLevel;
            LifeBase = lifeBase; LifePerLevel = lifePerLevel;
            Satiety = satiety; Fatigue = fatigue; Water = water;
            DigestiveTime = digestiveTime;
            EatMotion = eatMotion; EffectOn = effectOn; EffectSeconds = effectSeconds;
        }

        /// <summary>สตามินาที่ไอเทมเลเวลนี้ให้</summary>
        public float EnergyAt(int level) => EnergyBase + EnergyPerLevel * (level - 1);

        /// <summary>เลือดที่ไอเทมเลเวลนี้ให้</summary>
        public float HealthAt(int level) => HealthBase + HealthPerLevel * (level - 1);

        public float LifeAt(int level) => LifeBase + LifePerLevel * (level - 1);
    }

    private static Entry E(int minLevel, int maxLevel, float energyBase, float energyPerLevel,
        float healthBase, float healthPerLevel, float lifeBase, float lifePerLevel,
        float satiety, float fatigue, float water, int digestiveTime,
        string eatMotion, string effectOn, float effectSeconds)
        => new Entry(minLevel, maxLevel, energyBase, energyPerLevel, healthBase, healthPerLevel,
            lifeBase, lifePerLevel, satiety, fatigue, water, digestiveTime, eatMotion, effectOn, effectSeconds);

    /// <summary>prototype -> ข้อมูลอาหารแยกตามช่วงเลเวล</summary>
    public static readonly Dictionary<string, Entry[]> Map = new Dictionary<string, Entry[]>(StringComparer.Ordinal)
    {
%s
    };

    /// <summary>ของชิ้นนี้กินได้ไหม</summary>
    public static bool IsFood(string prototype)
    {
        return !string.IsNullOrEmpty(prototype) && Map.ContainsKey(prototype);
    }

    /// <summary>ข้อมูลอาหารของไอเทมเลเวลนี้ (ไม่เจอช่วงที่ตรง = ใช้ช่วงแรก)</summary>
    public static bool TryGet(string prototype, int level, out Entry entry)
    {
        entry = null;
        if (string.IsNullOrEmpty(prototype) || !Map.TryGetValue(prototype, out Entry[] entries) || entries.Length == 0)
        {
            return false;
        }
        for (int i = 0; i < entries.Length; i++)
        {
            if (level >= entries[i].MinLevel && level <= entries[i].MaxLevel)
            {
                entry = entries[i];
                return true;
            }
        }
        entry = entries[0];
        return true;
    }
}
''' % (len(foods), '\n'.join(rows))

OUT.write_text(src, encoding='utf-8')
print('เขียน %s แล้ว — อาหาร %d ชนิด (%d ช่วงเลเวล)' % (OUT, len(foods), count))
