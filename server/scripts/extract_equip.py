"""สกัดข้อมูล weapon/armor จาก resources.strings.txt → EquipData.cs

เดิมเอามาแค่ (model, slot, framework) พอให้ตัวละคร "หน้าตาเปลี่ยน" ตอนใส่ของ
รอบนี้เอา **ค่าพลังจริง** มาด้วย เพราะก่อนหน้านี้ใส่เกราะ = เปลี่ยนโมเดลเฉย ๆ
ไม่มีค่าป้องกันเลย และอาวุธทุกชิ้นบวกดาเมจเท่ากันหมด (ขวานหิน = ค้อนเหล็ก)

ข้อมูลที่เพิ่ม (อยู่ในบล็อกเดียวกันของเกมอยู่แล้ว):
  weapon: attack · attack_type · attack_cooltime · accuracy · attack_rating · critical
  armor : defense · bag_size

ค่าพวกนี้เป็น "สูตรของเลเวลไอเทม" เช่น "72.02 + (level * 1.3)" ทุกอันเป็นเส้นตรง
จึงเก็บเป็น (ค่าที่เลเวล 1, ค่าที่เพิ่มต่อ 1 เลเวล) แบบเดียวกับ FoodData

⚠️ ต่างจากรุ่นก่อน: **ไม่ตัดแถวที่ไม่มีโมเดลทิ้งแล้ว** เพราะตอนนี้แถวหนึ่งมีความหมาย
แม้ไม่มีโมเดล (ยังมีค่าป้องกัน/ช่องที่ใส่ได้) — ของที่ไม่มีโมเดลก็แค่ใส่แล้วไม่เห็นบนตัว

ใช้: python scripts/extract_equip.py <resources.strings.txt> <ServerCore/EquipData.cs>
"""
import json, re, sys, pathlib

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])
text = SRC.read_text(encoding='utf-8', errors='replace')
lines = text.split('\n')

BACKSLASH = chr(92)
QUOTE = chr(34)

# offset ของแต่ละบรรทัดในไฟล์เต็ม
_off = [0]
for _l in lines:
    _off.append(_off[-1] + len(_l) + 1)


def block_at(start_line_idx):
    """คืน JSON object ที่เริ่มจากบรรทัดนี้ ตัดตรงปีกกาปิดพอดี (ข้าม string literal)"""
    start = text.index('{', _off[start_line_idx])
    depth = 0
    i = start
    in_str = False
    n = len(text)
    while i < n:
        c = text[i]
        if in_str:
            if c == BACKSLASH:
                i += 2
                continue
            if c == QUOTE:
                in_str = False
        else:
            if c == QUOTE:
                in_str = True
            elif c == '{':
                depth += 1
            elif c == '}':
                depth -= 1
                if depth == 0:
                    return text[start:i + 1], i
        i += 1
    return None, i


def collect(kind):
    """รวมทุก block ของ kind ('weapon'/'armor') จากทุกตำแหน่งที่เจอ"""
    out = {}
    pat = '  "%s": {' % kind
    for idx, ln in enumerate(lines):
        if ln.rstrip() != pat:
            continue
        raw, _end = block_at(idx)
        if raw is None:
            continue
        try:
            obj = json.loads(raw)
        except Exception as e:
            print('  ข้าม block บรรทัด %d: %s' % (idx + 1, e))
            continue
        for proto, levels in obj.items():
            if not isinstance(levels, dict):
                continue
            # เอา entry ช่วงเลเวลแรก (โค้ดเกมก็ใช้ตัวแรกเหมือนกัน)
            for _lv, data in levels.items():
                if isinstance(data, dict):
                    out.setdefault(proto, data)
                    break
    return out


weapons = collect('weapon')
armors = collect('armor')
print('weapon: %d prototype, armor: %d prototype' % (len(weapons), len(armors)))


def esc(s):
    if s is None:
        return 'null'
    return QUOTE + s.replace(BACKSLASH, BACKSLASH * 2).replace(QUOTE, BACKSLASH + QUOTE) + QUOTE


def clean(v):
    """'None' ในข้อมูลเกม = ไม่มีโมเดล"""
    if v is None or v == 'None' or v == '':
        return None
    return v


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
        return float(eval(s, {'__builtins__': {}}, {'level': level}))    # noqa: S307 — กรองด้วย SAFE แล้ว
    except Exception:               # noqa: BLE001
        return 0.0


def linear(expr):
    """คืน (ค่าที่เลเวล 1, ค่าที่เพิ่มต่อ 1 เลเวล)"""
    a = value_at(expr, 1)
    b = value_at(expr, 2)
    return a, b - a


def g(x):
    return '%gf' % round(x, 4)


w_rows = []
for proto in sorted(weapons):
    d = weapons[proto]
    atk_base, atk_step = linear(d.get('attack'))
    rating_base, rating_step = linear(d.get('attack_rating'))
    acc_base, acc_step = linear(d.get('accuracy'))
    crit_base, _crit_step = linear(d.get('critical'))
    cool = value_at(d.get('attack_cooltime'), 1)
    w_rows.append('        { %s, W(%s, %s, %s, %s, %s, %s, %s, %s, %s, %s, %s) },' % (
        esc(proto),
        esc(clean(d.get('model'))), esc(clean(d.get('weapon_framework'))), esc(clean(d.get('slot'))),
        esc(clean(d.get('attack_type'))),
        g(atk_base), g(atk_step),
        g(rating_base), g(rating_step),
        g(acc_base), g(acc_step),
        g(crit_base)))

a_rows = []
for proto in sorted(armors):
    d = armors[proto]
    def_base, def_step = linear(d.get('defense'))
    bag = value_at(d.get('bag_size'), 1)
    a_rows.append('        { %s, A(%s, %s, %s, %s, %s, %s) },' % (
        esc(proto),
        esc(clean(d.get('male_model'))), esc(clean(d.get('female_model'))), esc(clean(d.get('slot'))),
        g(def_base), g(def_step), g(bag)))

src = '''using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// เฟส C — ข้อมูลอุปกรณ์: prototype ของไอเทม → โมเดล + **ค่าพลังจริงของชิ้นนั้น**
///
/// สกัดอัตโนมัติจาก game/DurangoV2_Data/resources.strings.txt (บล็อก "weapon" / "armor"
/// ของ performances) ด้วย scripts/extract_equip.py — **อย่าแก้ด้วยมือ** ให้รันสคริปต์ใหม่แทน
///
/// ในเกมจริง client อ่านข้อมูลนี้จาก YAML ฝั่งตัวเอง (PerformanceYaml) แต่ server ต้องรู้ด้วย
/// เพราะเป็นคนตัดสินว่าใส่ของแล้วตัวละครหน้าตาเปลี่ยนยังไง แล้ว broadcast ให้คนอื่นเห็น
/// และตั้งแต่ beta 1.0 เป็นต้นไป **เป็นคนคิดดาเมจ/ค่าป้องกันจากของชิ้นนั้นด้วย**
/// (ดู docs/server/Abilities.md — ค่าดิบพวกนี้ถูกคูณสเกลใน config.json ก่อนใช้จริง)
///
/// ค่าที่ขึ้นกับเลเวลไอเทมเก็บเป็น (ค่าที่เลเวล 1, ค่าที่เพิ่มต่อเลเวล) — สูตรในเกมเป็นเส้นตรงหมด
/// </summary>
public static class EquipData
{
    /// <summary>
    /// ช่องที่ไอเทมนี้ใส่ได้ (main/both/head/body/gloves/shoes/bag/precious/sub) — คืน null ถ้าใส่ไม่ได้
    ///
    /// 🐛 **สำคัญมาก:** client ตัดสินว่า "ใส่ได้ไหม" จาก `Performance.Strs["slot"]` ของไอเทม
    /// (EquipSystem.EquipItem → item.GetStringAttribute("slot") → ItemData._performances)
    /// ถ้าไม่มีค่านี้ มัน **return เงียบ ๆ ไม่ส่ง packet ไม่ขึ้น error อะไรเลย**
    /// ⇒ อาการคือกดใส่อาวุธแล้วไม่มีอะไรเกิดขึ้น
    /// เซิร์ฟจึงต้องแนบ Performance ที่มี slot ไปกับไอเทมทุกชิ้นที่ใส่ได้
    /// </summary>
    public static string SlotOf(string prototype)
    {
        if (string.IsNullOrEmpty(prototype))
        {
            return null;
        }
        if (Weapons.TryGetValue(prototype, out WeaponInfo w))
        {
            return w.Slot;
        }
        if (Armors.TryGetValue(prototype, out ArmorInfo a))
        {
            return a.Slot;
        }
        return null;
    }

    /// <summary>Performance ที่พก slot ไปให้ client — ไม่ใช่ของใส่ได้ก็คืน null</summary>
    public static Messages.Performance[] PerformanceFor(string prototype)
    {
        string slot = SlotOf(prototype);
        if (slot == null)
        {
            return null;
        }
        return new[]
        {
            new Messages.Performance
            {
                Id = "equip",
                Name = null,
                Icon = null,
                Nums = null,
                Strs = new Dictionary<string, string> { { "slot", slot } }
            }
        };
    }

    public readonly struct WeaponInfo
    {
        public readonly string Model;
        public readonly string Framework;
        public readonly string Slot;
        /// <summary>ชนิดการโจมตี (sword/axe/blunt/spear/arrow/stone/bare_hands)</summary>
        public readonly string AttackType;

        /// <summary>พลังโจมตีดิบของเกม — ยังไม่คูณสเกล (ดู CombatConfig.WeaponAttackScale)</summary>
        public readonly float AttackBase;
        public readonly float AttackPerLevel;
        /// <summary>ค่า "เรตติ้งโจมตี" ที่หน้าตัวละครโชว์</summary>
        public readonly float RatingBase;
        public readonly float RatingPerLevel;
        public readonly float AccuracyBase;
        public readonly float AccuracyPerLevel;
        /// <summary>โอกาสคริของอาวุธ (ข้อมูลเกมเป็น 0 เกือบทุกชิ้น)</summary>
        public readonly float Critical;

        public WeaponInfo(string model, string framework, string slot, string attackType,
            float attackBase, float attackPerLevel, float ratingBase, float ratingPerLevel,
            float accuracyBase, float accuracyPerLevel, float critical)
        {
            Model = model;
            Framework = framework;
            Slot = slot;
            AttackType = attackType;
            AttackBase = attackBase;
            AttackPerLevel = attackPerLevel;
            RatingBase = ratingBase;
            RatingPerLevel = ratingPerLevel;
            AccuracyBase = accuracyBase;
            AccuracyPerLevel = accuracyPerLevel;
            Critical = critical;
        }

        /// <summary>พลังโจมตีดิบที่เลเวลไอเทมนี้</summary>
        public float AttackAt(int level)
        {
            return AttackBase + AttackPerLevel * (level < 1 ? 0 : level - 1);
        }

        public float RatingAt(int level)
        {
            return RatingBase + RatingPerLevel * (level < 1 ? 0 : level - 1);
        }

        public float AccuracyAt(int level)
        {
            return AccuracyBase + AccuracyPerLevel * (level < 1 ? 0 : level - 1);
        }
    }

    public readonly struct ArmorInfo
    {
        public readonly string MaleModel;
        public readonly string FemaleModel;
        public readonly string Slot;

        /// <summary>ค่าป้องกันดิบของเกม — ยังไม่คูณสเกล (ดู CombatConfig.ArmorDefenseScale)</summary>
        public readonly float DefenseBase;
        public readonly float DefensePerLevel;
        /// <summary>ช่องกระเป๋าที่เพิ่มให้ (ข้อมูลเกมมีแค่ 2 ชิ้น — ยังไม่ได้ใช้)</summary>
        public readonly float BagSize;

        public ArmorInfo(string maleModel, string femaleModel, string slot,
            float defenseBase, float defensePerLevel, float bagSize)
        {
            MaleModel = maleModel;
            FemaleModel = femaleModel;
            Slot = slot;
            DefenseBase = defenseBase;
            DefensePerLevel = defensePerLevel;
            BagSize = bagSize;
        }

        /// <summary>ค่าป้องกันดิบที่เลเวลไอเทมนี้</summary>
        public float DefenseAt(int level)
        {
            return DefenseBase + DefensePerLevel * (level < 1 ? 0 : level - 1);
        }
    }

    private static WeaponInfo W(string model, string framework, string slot, string attackType,
        float attackBase, float attackPerLevel, float ratingBase, float ratingPerLevel,
        float accuracyBase, float accuracyPerLevel, float critical)
    {
        return new WeaponInfo(model, framework, slot, attackType, attackBase, attackPerLevel,
            ratingBase, ratingPerLevel, accuracyBase, accuracyPerLevel, critical);
    }

    private static ArmorInfo A(string maleModel, string femaleModel, string slot,
        float defenseBase, float defensePerLevel, float bagSize)
    {
        return new ArmorInfo(maleModel, femaleModel, slot, defenseBase, defensePerLevel, bagSize);
    }

    /// <summary>อาวุธ %d ชิ้น</summary>
    public static readonly Dictionary<string, WeaponInfo> Weapons = new Dictionary<string, WeaponInfo>
    {
%s
    };

    /// <summary>เสื้อผ้า/หมวก/ถุงมือ/รองเท้า %d ชิ้น</summary>
    public static readonly Dictionary<string, ArmorInfo> Armors = new Dictionary<string, ArmorInfo>
    {
%s
    };

    public static bool TryGetWeapon(string prototype, out WeaponInfo info)
    {
        return Weapons.TryGetValue(prototype ?? string.Empty, out info);
    }

    public static bool TryGetArmor(string prototype, out ArmorInfo info)
    {
        return Armors.TryGetValue(prototype ?? string.Empty, out info);
    }
}
''' % (len(w_rows), '\n'.join(w_rows), len(a_rows), '\n'.join(a_rows))

OUT.write_text(src, encoding='utf-8')
print('เขียน %s — อาวุธ %d, เกราะ %d' % (OUT.name, len(w_rows), len(a_rows)))
