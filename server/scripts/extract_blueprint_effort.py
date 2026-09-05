"""สกัด effort/energy/min_level ของ blueprint -> BlueprintEffortData.cs   (TodoList/04)

ต้นฉบับ: เวลาสร้าง = effort ของ blueprint (วินาที, duration_formula = "e") · สตามินา = energy
ถ้า blueprint ไม่ระบุ (effort 0) ใช้ constants.effort_standard.build = 10 + (level-1)×1

ใช้:  python server/scripts/extract_blueprint_effort.py server/data/assets/building/blueprints.json server/ServerCore/BlueprintEffortData.cs
"""
import json
import pathlib
import sys

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])
data = json.loads(SRC.read_text(encoding="utf-8"))

lines = []
w = lines.append
w("// สร้างด้วย scripts/extract_blueprint_effort.py จาก data/assets/building/blueprints.json — อย่าแก้มือ")
w("//")
w("// [TodoList/04] effort (วินาทีที่ใช้สร้าง) · energy (สตามินา) · min_level ของ blueprint ตามเกม")
w("")
w("using System;")
w("using System.Collections.Generic;")
w("")
w("namespace DurangoServer.Core;")
w("")
w("public static class BlueprintEffortData")
w("{")
w("    public readonly struct Info")
w("    {")
w("        public readonly float Effort, Energy;")
w("        public readonly int MinLevel;")
w("        public Info(float effort, float energy, int minLevel) { Effort = effort; Energy = energy; MinLevel = minLevel; }")
w("    }")
w("")
w("    public static readonly Dictionary<string, Info> All = new Dictionary<string, Info>(StringComparer.Ordinal)")
w("    {")
n = 0
for key in sorted(data):
    b = data[key]
    def num(v):
        try:
            return float(v)
        except (TypeError, ValueError):
            return 0.0
    effort = num(b.get("effort"))
    energy = num(b.get("energy"))
    lvl = int(num(b.get("min_level")) or 1)
    w("        {{ \"{0}\", new Info({1}f, {2}f, {3}) }},".format(key, effort, energy, lvl))
    n += 1
w("    };")
w("")
w("    public static bool TryGet(string blueprintId, out Info info)")
w("    {")
w("        if (blueprintId != null && All.TryGetValue(blueprintId, out info)) { return true; }")
w("        info = default; return false;")
w("    }")
w("}")
OUT.write_text("\n".join(lines) + "\n", encoding="utf-8")
print("blueprint %d" % n)
