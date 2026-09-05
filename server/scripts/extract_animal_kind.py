"""สกัด "ประเภท/คูลดาวน์กัด/ช่วงเลเวล" ของสัตว์รายชนิดจาก entity_types/animal.json -> AnimalKindData.cs

ทำไมต้องมี: ระบบฝูง (TodoList/08) เสกสัตว์ตาม region template ซึ่งมีชนิดที่ไม่อยู่ใน config Spawn
ของเรา — ต้องรู้ว่าตัวไหนดุ (Carnivore) ตัวไหนหนี และกัดถี่แค่ไหน โดยไม่ต้องเติม config ทีละตัว

ใช้:  python server/scripts/extract_animal_kind.py server/data/assets/entity_types/animal.json server/ServerCore/AnimalKindData.cs
"""
import json
import pathlib
import sys

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])

data = json.loads(SRC.read_text(encoding="utf-8"))
lines = []
w = lines.append
w("// สร้างด้วย scripts/extract_animal_kind.py จาก data/assets/entity_types/animal.json — อย่าแก้มือ")
w("//")
w("// type: Carnivore / Herbivore / Scavenger / Sandbag · attack_cooltime (วินาที) · combat_level_ranges")
w("")
w("using System.Collections.Generic;")
w("")
w("namespace DurangoServer.Core;")
w("")
w("public static class AnimalKindData")
w("{")
w("    public enum Kind { Herbivore, Carnivore, Scavenger, Sandbag }")
w("")
w("    public readonly struct Info")
w("    {")
w("        public readonly Kind Kind;")
w("        public readonly float AttackCooltime;")
w("        public readonly int LevelMin, LevelMax;")
w("        public Info(Kind kind, float cd, int lo, int hi) { Kind = kind; AttackCooltime = cd; LevelMin = lo; LevelMax = hi; }")
w("    }")
w("")
w("    private static Info I(Kind k, float cd, int lo, int hi) => new Info(k, cd, lo, hi);")
w("")
w("    public static readonly Dictionary<ushort, Info> All = new Dictionary<ushort, Info>()")
w("    {")
n = 0
for key in sorted(data, key=int):
    a = data[key]
    kind = a.get("type") or "Herbivore"
    if kind not in ("Herbivore", "Carnivore", "Scavenger", "Sandbag"):
        kind = "Herbivore"
    cd = float(a.get("attack_cooltime") or 0)
    rng = a.get("combat_level_ranges") or [1, 80]
    name = next(iter((a.get("name") or {"?": None}).keys()))
    w("        {{ {0}, I(Kind.{1}, {2}f, {3}, {4}) }},  // {5}".format(key, kind, cd, int(rng[0]), int(rng[1]), name))
    n += 1
w("    };")
w("")
w("    public static bool TryGet(ushort entityType, out Info info) => All.TryGetValue(entityType, out info);")
w("}")
OUT.write_text("\n".join(lines) + "\n", encoding="utf-8")
print("สกัดได้ %d ชนิด" % n)
