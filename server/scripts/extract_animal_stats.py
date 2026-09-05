"""สกัดสูตรพลังสัตว์รายชนิดจาก entity_types/animal.json -> AnimalStatData.cs

ทำไมต้องมี: เซิร์ฟคิดเลือด/ดาเมจสัตว์จากสูตรกลางสูตรเดียวใน config.json ใช้กับทั้ง 213 ชนิด
ทั้งที่ข้อมูลเกมมีสูตรรายชนิดพร้อมตัวเลขจริงอยู่แล้ว (เลือดต่างกัน 96 แบบ · ดาเมจ 86 แบบ)
บราคิโอควรเลือดหนากว่ากิ้งก่า ~8.5 เท่า แต่เดิมเท่ากันเป๊ะ

สูตรในไฟล์เกมมีแค่ 3 รูปแบบทั้ง 214 ชนิด (ตรวจแล้ว):
    life_max  = A * ((combat_level + B) ** C) * unstable_factor
    attack    = (B + combat_level * C) * unstable_factor      (2 ตัวคูณ unstable เฉพาะเทอมเลเวล)
    defense   = (B + combat_level * C) * unstable_factor

combat_level    = เลเวลของสัตว์ (ช่วง 1-80 ตรงกับ combat_level_ranges)
unstable_factor = ระดับความปั่นป่วนของเกาะ 1-8 — เกาะเราไม่มีระบบนี้ ใช้ 1 (ค่าฐาน)
                  ⇒ ทุกสูตรลดรูปเหลือค่าคงที่ที่สกัดออกมาได้ตรง ๆ ไม่ต้องมี evaluator

ใช้:  python server/scripts/extract_animal_stats.py server/data/assets/entity_types/animal.json server/ServerCore/AnimalStatData.cs
"""
import json
import pathlib
import re
import sys

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])

NUM = r"(\d+(?:\.\d+)?)"
# life: A * ((combat_level + B) ** C)   — วงเล็บนอก/ช่องว่างต่างกันได้
RE_LIFE = re.compile(
    r"^\(?\s*" + NUM + r"\s*\*\s*\(\(\s*combat_level\s*\+\s*" + NUM + r"\s*\)\s*\*\*\s*" + NUM + r"\s*\)\s*\)?\s*\*\s*unstable_factor\s*$")
# attack/defense: (B + combat_level * C) * unstable_factor
RE_LIN = re.compile(
    r"^\(\s*" + NUM + r"\s*\+\s*combat_level\s*\*\s*" + NUM + r"\s*\)\s*\*\s*unstable_factor\s*$")
# attack แบบที่ 2: B + (combat_level * C) * unstable_factor  (ที่ unstable=1 เท่ากับแบบแรก)
RE_LIN2 = re.compile(
    r"^" + NUM + r"\s*\+\s*\(\s*combat_level\s*\*\s*" + NUM + r"\s*\)\s*\*\s*unstable_factor\s*$")


def parse_life(s):
    m = RE_LIFE.match(str(s).strip())
    return (float(m.group(1)), float(m.group(2)), float(m.group(3))) if m else None


def parse_lin(s):
    s = str(s).strip()
    m = RE_LIN.match(s) or RE_LIN2.match(s)
    return (float(m.group(1)), float(m.group(2))) if m else None


def parse_num(s):
    try:
        return float(str(s).strip())
    except ValueError:
        return None


data = json.loads(SRC.read_text(encoding="utf-8"))
rows = []
skipped = []
for key in sorted(data, key=int):
    a = data[key]
    life = parse_life(a.get("life_max"))
    atk = parse_lin(a.get("attack"))
    dfn = parse_lin(a.get("defense"))
    crit = parse_num(a.get("critical"))
    if life is None or atk is None or dfn is None:
        skipped.append((key, a.get("life_max"), a.get("attack"), a.get("defense")))
        continue
    name = next(iter((a.get("name") or {"?": None}).keys()))
    rows.append((int(key), name, life, atk, dfn, crit if crit is not None else 0.0))

lines = []
w = lines.append
w("// สร้างด้วย scripts/extract_animal_stats.py จาก data/assets/entity_types/animal.json — อย่าแก้มือ")
w("//")
w("// สูตรพลังของสัตว์ *รายชนิด* ตามข้อมูลเกมจริง (ที่ unstable_factor = 1):")
w("//   เลือด   = LifeA * (level + LifeB) ^ LifeC")
w("//   โจมตี   = AtkBase + level * AtkPerLevel")
w("//   ป้องกัน = DefBase + level * DefPerLevel")
w("//")
w("// เซิร์ฟ *ไม่ได้* เอาตัวเลขดิบไปใช้ตรง ๆ — ใช้เป็น \"อัตราส่วนเทียบกับสัตว์อ้างอิง\" คูณสูตรกลาง")
w("// ใน config.json (ดู SpawnTable.LifeFor/DamageFor) เพื่อรักษาสมดุลที่จูนไว้แล้ว")
w("// แต่ให้แต่ละชนิดมีบุคลิกต่างกันจริง")
w("")
w("using System.Collections.Generic;")
w("")
w("namespace DurangoServer.Core;")
w("")
w("public static class AnimalStatData")
w("{")
w("    public readonly struct Stats")
w("    {")
w("        public readonly float LifeA, LifeB, LifeC;")
w("        public readonly float AtkBase, AtkPerLevel;")
w("        public readonly float DefBase, DefPerLevel;")
w("        public readonly float Critical;")
w("")
w("        public Stats(float lifeA, float lifeB, float lifeC, float atkBase, float atkPerLevel,")
w("                     float defBase, float defPerLevel, float critical)")
w("        {")
w("            LifeA = lifeA; LifeB = lifeB; LifeC = lifeC;")
w("            AtkBase = atkBase; AtkPerLevel = atkPerLevel;")
w("            DefBase = defBase; DefPerLevel = defPerLevel;")
w("            Critical = critical;")
w("        }")
w("")
w("        public float LifeAt(int level) => LifeA * System.MathF.Pow(level + LifeB, LifeC);")
w("        public float AttackAt(int level) => AtkBase + level * AtkPerLevel;")
w("        public float DefenseAt(int level) => DefBase + level * DefPerLevel;")
w("    }")
w("")
w("    private static Stats S(float lifeA, float lifeB, float lifeC, float atkBase, float atkPerLevel,")
w("                           float defBase, float defPerLevel, float critical)")
w("        => new Stats(lifeA, lifeB, lifeC, atkBase, atkPerLevel, defBase, defPerLevel, critical);")
w("")
w("    public static readonly Dictionary<ushort, Stats> All = new Dictionary<ushort, Stats>()")
w("    {")
for eid, name, (la, lb, lc), (ab, ap), (db, dp), crit in rows:
    w("        {{ {0}, S({1}f, {2}f, {3}f, {4}f, {5}f, {6}f, {7}f, {8}f) }},  // {9}".format(
        eid, la, lb, lc, ab, ap, db, dp, crit, name))
w("    };")
w("")
w("    public static bool TryGet(ushort entityType, out Stats stats)")
w("    {")
w("        return All.TryGetValue(entityType, out stats);")
w("    }")
w("}")
OUT.write_text("\n".join(lines) + "\n", encoding="utf-8")

print("สกัดได้ %d ชนิด · ข้าม %d" % (len(rows), len(skipped)))
for s in skipped[:10]:
    print("  ข้าม", s)
