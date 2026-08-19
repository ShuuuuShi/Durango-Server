"""สกัด "ข้อมูลรอบ ๆ สูตร" (TextAsset `recipes`) จาก resources.strings.txt -> RecipeMeta.cs

RecipeRequirements.cs เก็บแค่ "ช่องวัตถุดิบ" — ไฟล์นี้เก็บส่วนที่เหลือที่เกมใช้จริง:
  · category/subcategory  — cook / clothing / weapon_and_tool / ...  (ใช้เปิด-ปิดตาม Features)
  · duration / energy     — เวลาคราฟต์จริง + สตามินาที่เสียจริง (เดิม hardcode 2 วิ / 4 แต้ม)
  · count                 — คราฟต์ครั้งเดียวได้กี่ชิ้น (น้ำซุปได้ 2 ถ้วย)
  · min_level             — เลเวลขั้นต่ำของสูตร
  · workbench_tags        — ต้องยืนใกล้สิ่งปลูกสร้างที่มี tag นี้ (cook 15 = เตาที่ดีกว่ากองไฟ)
  · tool_tags             — ต้องถือเครื่องมือที่มี tag นี้ (pot / knife / grill_stone)
  · prototypes            — **ผลลัพธ์เปลี่ยนตามวัตถุดิบที่ใส่** (broth + meat = broth_meat)

ใช้: python scripts/extract_recipe_meta.py <resources.strings.txt> <ServerCore/RecipeMeta.cs>
"""
import io
import json
import sys
import pathlib

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])

lines = io.open(SRC, 'r', encoding='utf-8', errors='replace').read().split('\n')


def load_asset(name):
    """อ่าน TextAsset ชื่อ name — dump ตัดปีกกาชั้นนอกทิ้ง ต้องเติมเองแล้วลองปิดทีละชั้น"""
    start = None
    for i, line in enumerate(lines):
        if line.rstrip() == name:
            start = i + 1
            break
    if start is None:
        raise SystemExit('หา TextAsset `%s` ใน dump ไม่เจอ' % name)
    body_lines = []
    for line in lines[start:]:
        if line and not line[0].isspace():
            break
        body_lines.append(line)
    body = '\n'.join(body_lines).rstrip().rstrip(',')
    last = None
    for extra in range(1, 6):
        try:
            return json.loads('{' + body + '}' * extra)
        except Exception as e:      # noqa: BLE001 — ลองปิดปีกกาเพิ่มทีละชั้น
            last = e
    raise SystemExit('parse JSON ของ %s ไม่ได้: %s' % (name, last))


recipes = load_asset('recipes')


def num(value, default=0.0):
    """ค่าในข้อมูลเกมมีทั้ง "9" (string) และ 5 (int) — และบางช่องเป็นสูตรคำนวณ"""
    if value is None:
        return default
    try:
        return float(value)
    except (TypeError, ValueError):
        return default


def cs_str(value):
    if value is None:
        return 'null'
    return '"%s"' % str(value).replace('\\', '\\\\').replace('"', '\\"')


def cs_tags(dic):
    """{"cook": 40} -> new[] { T("cook",40) }  (ว่าง = null)"""
    if not dic:
        return 'null'
    inner = ', '.join('T(%s,%d)' % (cs_str(k), int(num(v, 1))) for k, v in sorted(dic.items()))
    return 'new[] { %s }' % inner


def cs_outputs(recipe):
    """prototypes[] -> ตัวเลือกผลลัพธ์ตามวัตถุดิบ (เรียงตามข้อมูล เจอตัวแรกที่ผ่านใช้ตัวนั้น)"""
    protos = recipe.get('prototypes') or []
    if not protos:
        return 'null'
    parts = []
    for p in protos:
        crit = []
        for c in (p.get('criteria') or []):
            cond = c.get('condition') or '>0'
            crit.append('C(%s,%s,%s)' % (cs_str(c.get('slot_id')), cs_str(c.get('tag_id')), cs_str(cond)))
        crit_arr = ('new[] { %s }' % ', '.join(crit)) if crit else 'null'
        parts.append('O(%s,%s)' % (cs_str(p.get('prototype_id')), crit_arr))
    return 'new[] { %s }' % ', '.join(parts)


out = []
out.append('using System.Collections.Generic;')
out.append('')
out.append('namespace DurangoServer.Core;')
out.append('')
out.append('// ข้อมูลรอบ ๆ สูตรคราฟต์ (generated จาก resources.strings.txt TextAsset `recipes`)')
out.append('// สร้างด้วย scripts/extract_recipe_meta.py — อย่าแก้มือ')
out.append('//')
out.append('// ช่องวัตถุดิบอยู่ที่ RecipeRequirements.cs — ไฟล์นี้คือส่วนที่เหลือ:')
out.append('// เวลา/สตามินา/จำนวนที่ได้/โต๊ะที่ต้องใช้/เครื่องมือที่ต้องถือ/ผลลัพธ์ที่เปลี่ยนตามวัตถุดิบ')
out.append('public static class RecipeMeta')
out.append('{')
out.append('    /// <summary>เงื่อนไขข้อเดียวของผลลัพธ์ — "ช่อง main มีของที่ติด tag meat มากกว่า 0 ชิ้น"</summary>')
out.append('    public sealed class Criterion')
out.append('    {')
out.append('        public readonly string SlotId;')
out.append('        public readonly string TagId;')
out.append('        /// <summary>"&gt;0" (ต้องมี) หรือ "&lt;0" (ต้องไม่มี) — ข้อมูลเกมมีแค่สองแบบนี้</summary>')
out.append('        public readonly string Condition;')
out.append('        public Criterion(string slotId, string tagId, string condition)')
out.append('        {')
out.append('            SlotId = slotId; TagId = tagId; Condition = condition;')
out.append('        }')
out.append('    }')
out.append('')
out.append('    /// <summary>ผลลัพธ์ทางเลือกหนึ่งอัน — เข้าเงื่อนไขครบทุกข้อถึงจะได้ prototype นี้</summary>')
out.append('    public sealed class Output')
out.append('    {')
out.append('        public readonly string PrototypeId;')
out.append('        public readonly Criterion[] Criteria;')
out.append('        public Output(string prototypeId, Criterion[] criteria)')
out.append('        {')
out.append('            PrototypeId = prototypeId; Criteria = criteria;')
out.append('        }')
out.append('    }')
out.append('')
out.append('    public sealed class Tag')
out.append('    {')
out.append('        public readonly string Id;')
out.append('        public readonly int Level;')
out.append('        public Tag(string id, int level) { Id = id; Level = level; }')
out.append('    }')
out.append('')
out.append('    public sealed class Info')
out.append('    {')
out.append('        public readonly string Category;')
out.append('        public readonly string Subcategory;')
out.append('        /// <summary>วินาทีที่ใช้คราฟต์ (ก่อนคูณสกิล)</summary>')
out.append('        public readonly float Duration;')
out.append('        /// <summary>สตามินาที่เสีย</summary>')
out.append('        public readonly float Energy;')
out.append('        /// <summary>คราฟต์ครั้งเดียวได้กี่ชิ้น</summary>')
out.append('        public readonly int Count;')
out.append('        public readonly int MinLevel;')
out.append('        /// <summary>0 = Craft (ได้ของใหม่) · 1 = Modify (แปรรูปของเดิม เช่น ย่าง/ต้ม) · 2 = Reform (แก้ทรงเสื้อ)</summary>')
out.append('        public readonly int Type;')
out.append('        /// <summary>prototype ตั้งต้นของผลลัพธ์ (ก่อนดู Outputs)</summary>')
out.append('        public readonly string PrototypeId;')
out.append('        /// <summary>ต้องมีโต๊ะ/เตาที่ติด tag ใด tag หนึ่งในนี้ (null = ไม่ต้องใช้โต๊ะ)</summary>')
out.append('        public readonly Tag[] Workbench;')
out.append('        /// <summary>ต้องถือเครื่องมือที่ติด tag ใด tag หนึ่งในนี้ (bare_hands = มือเปล่าก็ได้)</summary>')
out.append('        public readonly Tag[] Tools;')
out.append('        public readonly Output[] Outputs;')
out.append('')
out.append('        public Info(string category, string subcategory, float duration, float energy, int count,')
out.append('            int minLevel, int type, string prototypeId, Tag[] workbench, Tag[] tools, Output[] outputs)')
out.append('        {')
out.append('            Category = category; Subcategory = subcategory; Duration = duration; Energy = energy;')
out.append('            Count = count; MinLevel = minLevel; Type = type; PrototypeId = prototypeId;')
out.append('            Workbench = workbench; Tools = tools; Outputs = outputs;')
out.append('        }')
out.append('    }')
out.append('')
out.append('    private static Tag T(string id, int level) => new Tag(id, level);')
out.append('    private static Criterion C(string slot, string tag, string cond) => new Criterion(slot, tag, cond);')
out.append('    private static Output O(string proto, Criterion[] crit) => new Output(proto, crit);')
out.append('')
out.append('    private static Info I(string cat, string sub, float dur, float energy, int count,')
out.append('        int minLevel, int type, string proto, Tag[] wb, Tag[] tools, Output[] outputs)')
out.append('        => new Info(cat, sub, dur, energy, count, minLevel, type, proto, wb, tools, outputs);')
out.append('')
out.append('    public static readonly Dictionary<string, Info> Map = new Dictionary<string, Info>()')
out.append('    {')

for rid in sorted(recipes):
    r = recipes[rid]
    count = r.get('count')
    count = int(count) if count else 1
    out.append('        { %s, I(%s, %s, %sf, %sf, %d, %d, %d, %s, %s, %s, %s) },' % (
        cs_str(rid),
        cs_str(r.get('category')),
        cs_str(r.get('subcategory')),
        ('%g' % num(r.get('duration'))),
        ('%g' % num(r.get('energy'))),
        count,
        int(num(r.get('min_level'), 1)),
        int(num(r.get('type'), 0)),
        cs_str(r.get('prototype_id') or rid),
        cs_tags(r.get('workbench_tags')),
        cs_tags(r.get('tool_tags')),
        cs_outputs(r),
    ))

out.append('    };')
out.append('')
out.append('    public static bool TryGet(string recipeId, out Info info)')
out.append('    {')
out.append('        return Map.TryGetValue(recipeId ?? string.Empty, out info);')
out.append('    }')
out.append('}')

OUT.write_text('\n'.join(out) + '\n', encoding='utf-8')
print('เขียน %s แล้ว — %d สูตร' % (OUT, len(recipes)))
