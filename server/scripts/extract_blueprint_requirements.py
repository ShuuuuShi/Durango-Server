"""สกัดช่องวัตถุดิบของสิ่งปลูกสร้างจาก TextAsset `blueprints`.

ใช้: python scripts/extract_blueprint_requirements.py <resources.strings.txt> <ServerCore/BlueprintRequirements.cs>
"""
import io
import json
import pathlib
import sys

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])

lines = io.open(SRC, "r", encoding="utf-8", errors="replace").read().split("\n")
start = next((i + 1 for i, line in enumerate(lines) if line.rstrip() == "blueprints"), None)
if start is None:
    raise SystemExit("หา TextAsset `blueprints` ใน dump ไม่เจอ")

body = []
for line in lines[start:]:
    if line and not line[0].isspace():
        break
    body.append(line)

raw = "\n".join(body).rstrip().rstrip(",")
data = None
for extra in range(1, 5):
    try:
        data = json.loads("{" + raw + "}" * extra)
        break
    except json.JSONDecodeError:
        pass
if data is None:
    raise SystemExit("parse JSON ของ blueprints ไม่ได้")


def req(values):
    if not values:
        return "null"
    inner = ", ".join(
        'new TagRequirement("%s", %d)' % (str(key).replace("\\", "\\\\").replace('"', '\\"'), int(value or 1))
        for key, value in sorted(values.items())
    )
    return "new[] { %s }" % inner


out = [
    "using System.Collections.Generic;",
    "",
    "namespace DurangoServer.Core;",
    "",
    "// S1: ช่องวัตถุดิบของสิ่งปลูกสร้าง (generated จาก resources.strings.txt TextAsset `blueprints`)",
    "// สร้างด้วย scripts/extract_blueprint_requirements.py — อย่าแก้มือ",
    "public static class BlueprintRequirements",
    "{",
    "    public sealed class Slot",
    "    {",
    "        public readonly string Id;",
    "        public readonly int Min;",
    "        public readonly int Max;",
    "        public readonly TagRequirement[] Tags;",
    "        public readonly TagRequirement[] Materials;",
    "",
    "        public Slot(string id, int min, int max, TagRequirement[] tags, TagRequirement[] materials)",
    "        {",
    "            Id = id; Min = min; Max = max; Tags = tags; Materials = materials;",
    "        }",
    "    }",
    "",
    "    private static Slot S(string id, int count, TagRequirement[] tags = null, TagRequirement[] materials = null)",
    "    {",
    "        return new Slot(id, count, count, tags, materials);",
    "    }",
    "",
    "    public static readonly Dictionary<string, Slot[]> Blueprints = new Dictionary<string, Slot[]>()",
    "    {",
]
slots = 0
for blueprint_id in sorted(data):
    parts = []
    for slot in data[blueprint_id].get("slots") or []:
        count = int(slot.get("count", 0) or 0)
        if count <= 0:
            continue
        slots += 1
        parts.append('S("%s", %d, %s, %s)' % (
            slot.get("slot_id", "").replace("\\", "\\\\").replace('"', '\\"'), count,
            req(slot.get("required_tags") or {}), req(slot.get("required_materials") or {})))
    body_text = "new Slot[0]" if not parts else "new[] { %s }" % ", ".join(parts)
    out.append('        { "%s", %s },' % (blueprint_id.replace("\\", "\\\\").replace('"', '\\"'), body_text))
out.extend([
    "    };",
    "",
    "    /// <summary>คืน true แม้ blueprint ที่รู้จักจะไม่มีวัตถุดิบ</summary>",
    "    public static bool TryGet(string blueprintId, out Slot[] slots)",
    "    {",
    "        slots = null;",
    "        return !string.IsNullOrEmpty(blueprintId) && Blueprints.TryGetValue(blueprintId, out slots);",
    "    }",
    "}",
])
OUT.write_text("\n".join(out) + "\n", encoding="utf-8")
print("เขียน %s: %d blueprints, %d ช่องวัตถุดิบ" % (OUT, len(data), slots))
