"""สกัด "เงื่อนไขความสามารถของแต่ละสูตร" จาก TextAsset `recipes` → RecipeGateData.cs

ใช้: python scripts/extract_recipe_gate.py <resources.strings.txt> <ServerCore/RecipeGateData.cs>

ทำไมต้องมี: เดิม `GetRecipes` ส่งสูตรทั้ง 720 อันให้ทุกคนเสมอ ⇒ **ในเกมเห็นสูตรครบตั้งแต่เลเวล 1**
ในเกมจริง server ส่งเฉพาะสูตรที่ปลดล็อกแล้ว โดยแต่ละสูตรมีเงื่อนไข:

    "required_ability": 210          ← Shared.Ability.Derived (210 = Weaponcraft)
    "required_ability_value": 12     ← ต้องมีความสามารถนั้นถึงเท่านี้

ค่าที่พบในข้อมูลจริง (720 สูตร):
    210 Weaponcraft 212 · 217 Cook 157 · 211 Armorcraft 147 · 239 Handicraft 59
    219 Construction 39 · 215 Tailor 38 · 218 Furnishing 32 · 216 Smith 22 · 220 Farming 8
    (อีก 6 สูตรไม่มีเงื่อนไข = ได้ตั้งแต่แรก)
"""
import io
import json
import sys
import pathlib
from collections import Counter

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

body = []
for line in lines[start:]:
    if line and not line[0].isspace():
        break                       # เจอชื่อ asset ตัวถัดไปแล้ว
    body.append(line)

# dump ตัดปีกกาชั้นนอกทิ้ง — เติมเองแล้วลองปิดทีละชั้นจนกว่าจะ parse ผ่าน
text = '\n'.join(body)
recipes = None
for extra in range(0, 6):
    try:
        recipes = json.loads('{' + text + '}' * extra)
        break
    except Exception:
        continue
if recipes is None:
    raise SystemExit('parse ตาราง recipes ไม่ผ่าน')

rows = []
stat = Counter()
for rid in sorted(recipes):
    r = recipes[rid]
    ability = r.get('required_ability')
    value = r.get('required_ability_value')
    if ability is None:
        stat['ไม่มีเงื่อนไข'] += 1
        continue
    try:
        ability = int(ability)
        value = float(value or 0)
    except (TypeError, ValueError):
        stat['ค่าเสีย'] += 1
        continue
    stat[ability] += 1
    rows.append('        { "%s", (%d, %gf) },' % (rid.replace('"', ''), ability, value))

src = '''using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// เงื่อนไข "ต้องมีความสามารถเท่าไรถึงจะได้สูตรนี้" — ค่าจริงจากข้อมูลเกม
///
/// สกัดอัตโนมัติด้วย scripts/extract_recipe_gate.py · **อย่าแก้ด้วยมือ**
///
/// ใช้ที่ `GetRecipes` เพื่อส่งเฉพาะสูตรที่ผู้เล่นปลดล็อกแล้ว
/// เดิมส่งทั้ง %d สูตรให้ทุกคนเสมอ ⇒ เลเวล 1 ก็เห็นสูตรครบทุกอัน
///
/// สูตรที่ไม่มีในตารางนี้ = ไม่มีเงื่อนไข ได้ตั้งแต่แรก
/// </summary>
public static class RecipeGateData
{
    /// <summary>รหัสสูตร → (ความสามารถที่ต้องใช้ (Shared.Ability.Derived), ค่าที่ต้องถึง)</summary>
    public static readonly Dictionary<string, (int Ability, float Value)> Required =
        new Dictionary<string, (int, float)>
    {
%s
    };

    public static bool TryGet(string recipeId, out int ability, out float value)
    {
        if (recipeId != null && Required.TryGetValue(recipeId, out (int Ability, float Value) req))
        {
            ability = req.Ability;
            value = req.Value;
            return true;
        }
        ability = 0;
        value = 0f;
        return false;
    }
}
''' % (len(recipes), '\n'.join(rows))

OUT.write_text(src, encoding='utf-8')
print('เขียน %s — %d สูตรที่มีเงื่อนไข (จากทั้งหมด %d)' % (OUT.name, len(rows), len(recipes)))
print('แยกตามความสามารถ:', dict(stat))
