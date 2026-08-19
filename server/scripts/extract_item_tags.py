"""สกัด tag ของไอเทมจาก resources.strings.txt → ItemTagData.cs

โครงในข้อมูลเกม (prototype_data):
    "wood_log": [ { "category": "plant_collectible", "tags": { "wood": null, "pillar_normal": null }, ... } ]

tag คือสิ่งที่ทำให้ระบบอื่นรู้จักไอเทม:
  · สูตรคราฟต์ต้องการ tag ("blade" "rope" "handle" "chunk_big")
  · เครื่องมือเก็บของต้องการ tag ("axe" "knife" "pickaxe" — ดูหมายเหตุข้างล่าง)

หมายเหตุ: ข้อมูลเกมมี tag เครื่องมือแบบรวม ๆ ว่า "tool" อย่างเดียว ไม่ได้แยกขวาน/มีด/อีเต้อ
(ตารางที่แยกอยู่ฝั่ง server ของ NEXON ไม่ได้ติดมากับ client) — server เราจึงเติม tag
ชนิดเครื่องมือเองจากชื่อ prototype ดู TOOL_KINDS ข้างล่าง

ใช้: python scripts/extract_item_tags.py "../game/DurangoV2_Data/resources.strings.txt" ServerCore/ItemTagData.cs
"""
import json, re, sys, pathlib

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])
text = SRC.read_text(encoding='utf-8', errors='replace')
lines = text.split('\n')
_off = [0]
for _l in lines:
    _off.append(_off[-1] + len(_l) + 1)


def block_at(line_idx, opener='['):
    start = text.index(opener, _off[line_idx])
    close = ']' if opener == '[' else '}'
    depth = 0
    i = start
    in_str = False
    n = len(text)
    while i < n:
        c = text[i]
        if in_str:
            if c == chr(92):
                i += 2
                continue
            if c == chr(34):
                in_str = False
        else:
            if c == chr(34):
                in_str = True
            elif c in '[{':
                depth += 1
            elif c in ']}':
                depth -= 1
                if depth == 0:
                    return text[start:i + 1]
        i += 1
    return None


# ชนิดเครื่องมือที่ server เราแยกเอง: (คำที่อยู่ในชื่อ prototype, tag ที่จะเติมให้)
TOOL_KINDS = [
    ('pickaxe', 'pickaxe'),
    ('shovel', 'shovel'),
    ('hammer', 'hammer'),
    ('axe', 'axe'),
    ('knife', 'knife'),
    ('sword_tool', 'knife'),      # มีดแล่ในเกมคือ "sword_tool" (ดาบสำหรับใช้งาน)
    ('sickle', 'sickle'),
]

# ระดับเครื่องมือตามวัสดุ — หินต่ำสุด กระดูกกลาง โลหะสูงสุด
MATERIAL_LEVEL = [('metal', 3), ('bone', 2), ('stone', 1), ('wooden', 1), ('wood', 1)]

items = {}
pat = re.compile(r'^  "([a-z_0-9]+)": \[\s*$')
for idx, ln in enumerate(lines):
    m = pat.match(ln)
    if not m:
        continue
    proto = m.group(1)
    if proto in items:
        continue
    raw = block_at(idx, '[')
    if raw is None or '"tags"' not in raw:
        continue
    try:
        arr = json.loads(raw)
    except Exception:
        continue
    if not arr or not isinstance(arr[0], dict):
        continue
    obj = arr[0]
    tags = obj.get('tags')
    if not isinstance(tags, dict) or not tags:
        continue

    out = {}
    for tag, lv in tags.items():
        try:
            out[tag] = max(1, int(float(lv))) if lv is not None else 1
        except Exception:
            out[tag] = 1
    items[proto] = out

print('เจอไอเทมที่มี tag %d ชนิด' % len(items))

# เติม tag ชนิดเครื่องมือให้ prototype ที่หน้าตาเป็นเครื่องมือ
added = 0
for proto in list(items):
    for word, tag in TOOL_KINDS:
        if word in proto:
            if tag not in items[proto]:
                lvl = 1
                for mat, n in MATERIAL_LEVEL:
                    if mat in proto:
                        lvl = n
                        break
                items[proto][tag] = lvl
                items[proto].setdefault('tool', lvl)
                added += 1
            break
print('เติม tag ชนิดเครื่องมือให้อีก %d ชิ้น' % added)

rows = []
for proto in sorted(items):
    pairs = ', '.join('T("%s",%d)' % (t, lv) for t, lv in sorted(items[proto].items()))
    rows.append('        { "%s", new[] { %s } },' % (proto, pairs))

src = '''using System.Collections.Generic;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// tag ของไอเทม — **สร้างอัตโนมัติ อย่าแก้ด้วยมือ**
/// (`python scripts/extract_item_tags.py "../game/DurangoV2_Data/resources.strings.txt" ServerCore/ItemTagData.cs`)
///
/// tag คือสิ่งที่ทำให้ระบบอื่นรู้จักไอเทม:
///   · สูตรคราฟต์ต้องการ tag ("blade" "rope" "handle" "chunk_big") ไม่ได้ระบุชื่อไอเทมตรง ๆ
///   · การเก็บของ/แล่เนื้อต้องการเครื่องมือที่มี tag ตรงกับที่ generator ขอ
///
/// ⚠️ ข้อมูลเกมมี tag เครื่องมือรวม ๆ แค่ "tool" ไม่ได้แยกขวาน/มีด/อีเต้อ
/// (ตารางที่แยกอยู่ฝั่ง server ของ NEXON ไม่ได้ติดมากับ client)
/// สคริปต์จึงเติม tag ชนิดเครื่องมือ (axe/knife/pickaxe/shovel/hammer/sickle) จากชื่อ prototype
/// และให้ระดับตามวัสดุ: หิน/ไม้ = 1 · กระดูก = 2 · โลหะ = 3
/// </summary>
public static class ItemTagData
{
    private static Tag T(string id, int level) => new Tag { Id = id, Level = level };

    /// <summary>prototype → tag ทั้งหมดของไอเทมชิ้นนั้น</summary>
    public static readonly Dictionary<string, Tag[]> Map = new Dictionary<string, Tag[]>
    {
%s
    };

    /// <summary>tag ของไอเทมชิ้นนี้ (ไม่รู้จัก = ไม่มี tag เลย ไม่ใช่ null)</summary>
    public static Tag[] For(string prototype)
    {
        if (prototype != null && Map.TryGetValue(prototype, out Tag[] tags))
        {
            return tags;
        }
        return System.Array.Empty<Tag>();
    }

    /// <summary>ไอเทมชิ้นนี้มี tag นี้ระดับเท่าไร (0 = ไม่มี)</summary>
    public static int LevelOf(string prototype, string tag)
    {
        Tag[] tags = For(prototype);
        for (int i = 0; i < tags.Length; i++)
        {
            if (tags[i].Id == tag)
            {
                return tags[i].Level;
            }
        }
        return 0;
    }
}
''' % '\n'.join(rows)

OUT.write_text(src, encoding='utf-8')
print('เขียน %s (%d บรรทัด)' % (OUT, len(rows)))
