"""สกัด "ใบสั่งเกิดสัตว์" ของแต่ละเกาะจาก region_templates.json -> RegionTemplateData.cs

ทำไมต้องมี: เกมต้นฉบับกำหนดว่าเกาะไหนมีสัตว์ชนิดใด กี่ฝูง ฝูงละกี่ตัว ไว้ใน region template
(262 เกาะ) — ของเราเคยเลือกเอง 10 ชนิด/34 ตัว ซึ่งตรงกับต้นฉบับแค่ 4 ชนิด

รหัสฝูง = ชนิด 4 หลัก + จำนวนตัว 2 หลัก (ยืนยันจาก client AnimalCheatWidget:
cheat `spawn animal herd {entityType}{01|02|05|10|20|30}`)  เช่น 201520 = คอมป์ซอกนาทัส ฝูงละ 20

ใช้:  python server/scripts/extract_region_templates.py server/data/assets/region_templates.json server/ServerCore/RegionTemplateData.cs
"""
import json
import pathlib
import sys
from collections import Counter

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])

data = json.loads(SRC.read_text(encoding="utf-8"))

lines = []
w = lines.append
w("// สร้างด้วย scripts/extract_region_templates.py จาก data/assets/region_templates.json — อย่าแก้มือ")
w("//")
w("// \"ใบสั่งเกิดสัตว์\" ของแต่ละเกาะตามเกมต้นฉบับ: ฝูงต่อกลุ่มพื้นที่ (land/beach/lake_*/ocean)")
w("// รหัสฝูงในไฟล์เกม = ชนิด×100 + จำนวนตัว (201520 = ชนิด 2015 ฝูงละ 20) — ถอดไว้ให้แล้ว")
w("// ชื่อ template = <terrain><YYMMDD> หรือ <terrain>SubNN — ดู RegionTemplateData.Find")
w("")
w("using System;")
w("using System.Collections.Generic;")
w("")
w("namespace DurangoServer.Core;")
w("")
w("public static class RegionTemplateData")
w("{")
w("    public readonly struct HerdSpec")
w("    {")
w("        /// <summary>กลุ่มพื้นที่ใน herds.yml: land / beach / lake_shallow / lake_deep / ocean</summary>")
w("        public readonly string Group;")
w("        public readonly ushort EntityType;")
w("        /// <summary>ตัวต่อฝูง (2 หลักท้ายของรหัสฝูง)</summary>")
w("        public readonly int Size;")
w("        /// <summary>กี่ฝูง</summary>")
w("        public readonly int Count;")
w("        public HerdSpec(string group, ushort entityType, int size, int count)")
w("        {")
w("            Group = group; EntityType = entityType; Size = size; Count = count;")
w("        }")
w("    }")
w("")
w("    public sealed class Template")
w("    {")
w("        public string Name = \"\";")
w("        public int Level;")
w("        public int DesiredPopulation;")
w("        public HerdSpec[] Herds = Array.Empty<HerdSpec>();")
w("        /// <summary>หลุมอุกกาบาต: ชนิดที่สุ่ม (ratio) และจำนวนหลุม</summary>")
w("        public (ushort EntityType, float Ratio)[] CraterSpecies = Array.Empty<(ushort, float)>();")
w("        public int CraterCount;")
w("        /// <summary>รหัสฝูงเต็ม (ชนิด×100+ตัว) สำหรับหลุมอุกกาบาตที่ปิด</summary>")
w("        public int ClosedCraterHerdType;")
w("        public int TotalAnimals")
w("        {")
w("            get { int n = 0; foreach (HerdSpec h in Herds) { n += h.Size * h.Count; } return n; }")
w("        }")
w("    }")
w("")
w("    private static HerdSpec H(string g, ushort t, int s, int c) => new HerdSpec(g, t, s, c);")
w("")
w("    public static readonly Dictionary<string, Template> All = new Dictionary<string, Template>(StringComparer.Ordinal)")
w("    {")

n_templates = 0
for name in sorted(data):
    t = data[name]
    herds = t.get("herds") or {}
    specs = []
    for group in ("land", "beach", "lake_shallow", "lake_deep", "ocean"):
        g = herds.get(group)
        if not g:
            continue
        for code, count in sorted(Counter(g.get("spawns") or []).items()):
            code = int(code)
            specs.append((group, code // 100, code % 100, count))
    craters = (t.get("biocoms") or {}).get("craters") or {}
    ratios = sorted((int(k), float(v)) for k, v in (craters.get("spawn_ratios") or {}).items())
    crater_count = int(craters.get("total_count") or 0)
    closed_raw = t.get("closed_crater_herd_type")
    closed = int(closed_raw) if str(closed_raw).isdigit() else 0
    level = int(t.get("level") or 0)
    pop = int(t.get("desired_population") or 0)
    if not specs and crater_count == 0:
        continue  # เกาะที่ไม่มีสัตว์เลย (เช่น เกาะส่วนตัว pe*) ไม่ต้องใส่ตาราง
    n_templates += 1
    w("        {{ \"{0}\", new Template".format(name))
    w("        {")
    w("            Name = \"{0}\", Level = {1}, DesiredPopulation = {2},".format(name, level, pop))
    if specs:
        w("            Herds = new[] {")
        for group, et, size, count in specs:
            w("                H(\"{0}\", {1}, {2}, {3}),".format(group, et, size, count))
        w("            },")
    if ratios:
        w("            CraterSpecies = new (ushort, float)[] {{ {0} }},".format(
            ", ".join("({0}, {1}f)".format(et, r) for et, r in ratios)))
    w("            CraterCount = {0}, ClosedCraterHerdType = {1},".format(crater_count, closed))
    w("        } },")
w("    };")
w("")
w("    /// <summary>")
w("    /// หา template ให้ terrain id (เช่น \"ri35te\"): ตรงชื่อเป๊ะก่อน · ไม่งั้นเอาชื่อที่ขึ้นต้นด้วย id")
w("    /// แล้วตามด้วยวันที่ 6 หลัก เวอร์ชันล่าสุด (ri35te171228 > ri35te170615) · ไม่มีเลยคืน null")
w("    /// </summary>")
w("    public static Template Find(string terrainId)")
w("    {")
w("        if (string.IsNullOrEmpty(terrainId)) { return null; }")
w("        if (All.TryGetValue(terrainId, out Template exact)) { return exact; }")
w("        Template best = null;")
w("        foreach (KeyValuePair<string, Template> kv in All)")
w("        {")
w("            string n = kv.Key;")
w("            if (n.Length != terrainId.Length + 6 || !n.StartsWith(terrainId, StringComparison.Ordinal)) { continue; }")
w("            bool digits = true;")
w("            for (int i = terrainId.Length; i < n.Length; i++) { if (!char.IsDigit(n[i])) { digits = false; break; } }")
w("            if (!digits) { continue; }")
w("            if (best == null || string.CompareOrdinal(n, best.Name) > 0) { best = kv.Value; }")
w("        }")
w("        return best;")
w("    }")
w("}")
OUT.write_text("\n".join(lines) + "\n", encoding="utf-8")
print("สกัด template ที่มีสัตว์ %d จาก %d เกาะ" % (n_templates, len(data)))
