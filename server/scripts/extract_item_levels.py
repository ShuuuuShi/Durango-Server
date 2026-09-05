"""สกัดช่วงเลเวลไอเทม + น้ำหนักช่องวัตถุดิบ -> ItemLevelData.cs   (TodoList/02)

ต้นฉบับ: เลเวลผลลัพธ์คราฟต์ = ค่าเฉลี่ยเลเวลวัสดุถ่วงด้วย `weight` ของแต่ละช่อง (recipes.json)
แล้ว clamp ด้วย `min_level..max_level` ของ prototype ผลลัพธ์ (prototype_data.json) และ `max_level` ของสูตร

ใช้:  python server/scripts/extract_item_levels.py server/data/assets/item/prototype_data.json server/data/assets/item/recipes.json server/ServerCore/ItemLevelData.cs
"""
import json
import pathlib
import sys

PROTO = pathlib.Path(sys.argv[1])
RECIPES = pathlib.Path(sys.argv[2])
OUT = pathlib.Path(sys.argv[3])

protos = json.loads(PROTO.read_text(encoding="utf-8"))
recipes = json.loads(RECIPES.read_text(encoding="utf-8"))

lines = []
w = lines.append
w("// สร้างด้วย scripts/extract_item_levels.py จาก item/prototype_data.json + item/recipes.json — อย่าแก้มือ")
w("//")
w("// [TodoList/02] ช่วงเลเวลของ prototype (min/max) · เพดานเลเวลของสูตร · น้ำหนักช่องวัตสดุที่ไม่ใช่ 1")
w("// เลเวลผลลัพธ์ = Σ(เลเวลวัสดุ × weight) / Σ weight  → clamp(min_level, min(max_level ของ prototype, ของสูตร))")
w("")
w("using System;")
w("using System.Collections.Generic;")
w("")
w("namespace DurangoServer.Core;")
w("")
w("public static class ItemLevelData")
w("{")
w("    /// <summary>prototype → (MinLevel, MaxLevel)</summary>")
w("    public static readonly Dictionary<string, (int Min, int Max)> Prototypes = new Dictionary<string, (int, int)>(StringComparer.Ordinal)")
w("    {")
n_p = 0
for key in sorted(protos):
    v = protos[key]
    if isinstance(v, list):
        v = v[0] if v else {}
    lo = v.get("min_level")
    hi = v.get("max_level")
    if lo is None and hi is None:
        continue
    lo = int(lo or 1)
    hi = int(hi or max(lo, 1))
    k = key.strip("  	")   # ข้อมูลเกมมีคีย์ติด nbsp เช่น "event_mapae "
    w("        {{ \"{0}\", ({1}, {2}) }},".format(k, lo, hi))
    n_p += 1
w("    };")
w("")
w("    /// <summary>สูตร → max_level ของสูตร (0 = ไม่จำกัด)</summary>")
w("    public static readonly Dictionary<string, int> RecipeMaxLevel = new Dictionary<string, int>(StringComparer.Ordinal)")
w("    {")
n_r = 0
weights = []
for key in sorted(recipes):
    r = recipes[key]
    ml = int(r.get("max_level") or 0)
    if ml > 0:
        w("        {{ \"{0}\", {1} }},".format(key, ml))
        n_r += 1
    for slot in r.get("slots") or []:
        wt = slot.get("weight", 1)
        try:
            wt = float(wt)
        except (TypeError, ValueError):
            wt = 1.0
        if wt != 1.0:
            weights.append((key, slot.get("slot_id") or "", wt))
w("    };")
w("")
w("    /// <summary>น้ำหนักช่องวัตถุดิบที่ไม่ใช่ 1 (\"recipe|slot\" → weight) — ช่องอื่น = 1 · weight 0 = ไม่มีผลต่อเลเวล</summary>")
w("    public static readonly Dictionary<string, float> SlotWeights = new Dictionary<string, float>(StringComparer.Ordinal)")
w("    {")
for key, slot, wt in weights:
    w("        {{ \"{0}|{1}\", {2}f }},".format(key, slot, wt))
w("    };")
w("")
w("    /// <summary>[TodoList/07] prototype ที่ dump_locked (ของเควสต์/อีเวนต์/ตั๋ว) — ทิ้งไม่ได้ ตายก็ไม่หล่น</summary>")
w("    public static readonly HashSet<string> DumpLocked = new HashSet<string>(StringComparer.Ordinal)")
w("    {")
for key in sorted(protos):
    v = protos[key]
    if isinstance(v, list):
        v = v[0] if v else {}
    if v.get("dump_locked"):
        w("        \"{0}\",".format(key.strip(" \u00a0\t")))
w("    };")
w("")
w("    public static bool TryGetRange(string prototype, out int min, out int max)")
w("    {")
w("        if (prototype != null && Prototypes.TryGetValue(prototype, out (int Min, int Max) r)) { min = r.Min; max = r.Max; return true; }")
w("        min = 1; max = 0; return false;")
w("    }")
w("")
w("    public static float WeightOf(string recipeId, string slotId)")
w("    {")
w("        return SlotWeights.TryGetValue((recipeId ?? \"\") + \"|\" + (slotId ?? \"\"), out float w) ? w : 1f;")
w("    }")
w("}")
OUT.write_text("\n".join(lines) + "\n", encoding="utf-8")
print("prototype %d · สูตรที่มี max_level %d · ช่อง weight≠1 %d" % (n_p, n_r, len(weights)))
