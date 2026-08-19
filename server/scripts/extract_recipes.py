"""สกัดวัตถุดิบที่แต่ละสูตรต้องใช้ (TextAsset `recipes`) จาก resources.strings.txt → RecipeRequirements.cs

ใช้: python scripts/extract_recipes.py <resources.strings.txt> <ServerCore/RecipeRequirements.cs>

รูปแบบใน dump: บรรทัดที่มีคำว่า `recipes` โดด ๆ = ชื่อ asset จากนั้นเป็นเนื้อ JSON
ที่ "ไม่มีปีกกาเปิด/ปิดชั้นนอก" (ตัว dump ตัดทิ้ง) จึงต้องเติมเองแล้วลองปิดทีละชั้นจนกว่าจะ parse ผ่าน
"""
import io
import json
import sys
import pathlib

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])

lines = io.open(SRC, 'r', encoding='utf-8', errors='replace').read().split('\n')

start = None
for i, line in enumerate(lines):
    if line.rstrip() == 'recipes':
        start = i + 1
        break
if start is None:
    raise SystemExit('หา TextAsset `recipes` ใน dump ไม่เจอ')

body_lines = []
for line in lines[start:]:
    if line and not line[0].isspace():
        break                       # เจอชื่อ asset ตัวถัดไปแล้ว
    body_lines.append(line)

body = '\n'.join(body_lines).rstrip().rstrip(',')
data = None
last_err = None
for extra in range(1, 5):
    try:
        data = json.loads('{' + body + '}' * extra)
        break
    except Exception as e:            # noqa: BLE001 — ลองปิดปีกกาเพิ่มทีละชั้น
        last_err = e
if data is None:
    raise SystemExit('parse JSON ของ recipes ไม่ได้: %s' % last_err)


def cs_array(values):
    """คืน literal array ของ C# (หรือ null ถ้าว่าง)"""
    if not values:
        return 'null'
    inner = ', '.join('"%s"' % v.replace('\\', '\\\\').replace('"', '\\"') for v in values)
    return 'new[] { %s }' % inner


out = []
out.append('using System.Collections.Generic;')
out.append('')
out.append('namespace DurangoServer.Core;')
out.append('')
out.append('// GP-08: ช่องวัตถุดิบของแต่ละสูตร (generated จาก resources.strings.txt TextAsset `recipes`)')
out.append('// สร้างด้วย scripts/extract_recipes.py — อย่าแก้มือ')
out.append('//')
out.append('// Min/Max = count_min/count_max ของช่องนั้น (0 = ใส่หรือไม่ใส่ก็ได้)')
out.append('// Tags/Materials = required_tags/required_materials ตามข้อมูลของเกม')
out.append('// (ไอเทมที่ server รุ่นนี้สร้างยังไม่มี tag ติดตัว จึงใช้เป็นข้อมูลประกอบ/log เท่านั้น)')
out.append('public static class RecipeRequirements')
out.append('{')
out.append('    public sealed class Slot')
out.append('    {')
out.append('        public readonly string Id;')
out.append('        public readonly int Min;')
out.append('        public readonly int Max;')
out.append('        public readonly string[] Tags;')
out.append('        public readonly string[] Materials;')
out.append('')
out.append('        public Slot(string id, int min, int max, string[] tags, string[] materials)')
out.append('        {')
out.append('            Id = id;')
out.append('            Min = min;')
out.append('            Max = max;')
out.append('            Tags = tags;')
out.append('            Materials = materials;')
out.append('        }')
out.append('    }')
out.append('')
out.append('    private static Slot S(string id, int min, int max, string[] tags = null, string[] materials = null)')
out.append('    {')
out.append('        return new Slot(id, min, max, tags, materials);')
out.append('    }')
out.append('')
out.append('    public static readonly Dictionary<string, Slot[]> Recipes = new Dictionary<string, Slot[]>()')
out.append('    {')

slot_count = 0
for recipe_id in sorted(data):
    recipe = data[recipe_id]
    slots = recipe.get('slots') or []
    parts = []
    for slot in slots:
        slot_count += 1
        parts.append('S("%s", %d, %d, %s, %s)' % (
            slot.get('slot_id', ''),
            int(slot.get('count_min', 0) or 0),
            int(slot.get('count_max', 0) or 0),
            cs_array(sorted((slot.get('required_tags') or {}).keys())),
            cs_array(sorted((slot.get('required_materials') or {}).keys())),
        ))
    body_txt = 'new Slot[0]' if not parts else 'new[] { %s }' % ', '.join(parts)
    out.append('        { "%s", %s },' % (recipe_id, body_txt))

out.append('    };')
out.append('')
out.append('    /// <summary>ช่องวัตถุดิบของสูตรนี้ — false ถ้าไม่มีสูตรนี้ในเกม</summary>')
out.append('    public static bool TryGet(string recipeId, out Slot[] slots)')
out.append('    {')
out.append('        slots = null;')
out.append('        return !string.IsNullOrEmpty(recipeId) && Recipes.TryGetValue(recipeId, out slots);')
out.append('    }')
out.append('}')
out.append('')

OUT.write_text('\n'.join(out), encoding='utf-8')
print('เขียน %s: %d สูตร, %d ช่องวัตถุดิบ' % (OUT, len(data), slot_count))
