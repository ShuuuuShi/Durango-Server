"""สกัดชื่อ/ไอคอนของไอเทมจาก resources.strings.txt (TextAsset `prototype_data`) -> ItemNameData.cs

ทำไมต้องมี: ไอเทมที่ server สร้างเอาชื่อมาจาก **ชื่อสูตร** (RecipeData.RecipeInfo)
ซึ่งใช้ได้กับสูตรที่ผลลัพธ์มีอย่างเดียว แต่พอสูตรมีผลลัพธ์หลายแบบตามวัตถุดิบ
(สูตร "broth" ใส่เนื้อได้ broth_meat / ใส่ผักได้ broth_vege) ชื่อจะกลายเป็น "육수 내기" ทั้งคู่
ตารางนี้ทำให้ทุก prototype มีชื่อ+ไอคอนของตัวเอง

โครงในข้อมูลเกม:
    "broth_meat": [ { "name": {"고기 육수": null}, "icon": "cook_broth_01", ... } ]

ใช้: python scripts/extract_item_names.py <resources.strings.txt> <ServerCore/ItemNameData.cs>
"""
import json
import re
import sys
import pathlib

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])
text = SRC.read_text(encoding='utf-8', errors='replace')
lines = text.split('\n')
_off = [0]
for _l in lines:
    _off.append(_off[-1] + len(_l) + 1)


def block_at(line_idx, opener='['):
    """ตัดก้อน JSON ที่เริ่มบรรทัดนี้ออกมา (dump ไม่มีปีกกาชั้นนอก จึงหาเอาเองทีละก้อน)"""
    start = text.index(opener, _off[line_idx])
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


def first_key(dic):
    """ชื่อในข้อมูลเกมเก็บเป็น {"고기 육수": null} — เอา key ตัวแรก"""
    if isinstance(dic, dict):
        for k in dic:
            if k:
                return k
    return None


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
    if raw is None or '"name"' not in raw:
        continue
    try:
        arr = json.loads(raw)
    except Exception:               # noqa: BLE001
        continue
    if not arr or not isinstance(arr[0], dict):
        continue
    obj = arr[0]
    name = first_key(obj.get('name'))
    icon = obj.get('icon') or ''
    if not name:
        continue
    items[proto] = (name, icon)


def cs_str(value):
    if value is None or value == '':
        return 'null'
    return '"%s"' % str(value).replace('\\', '\\\\').replace('"', '\\"')


rows = ['        { %s, (%s, %s) },' % (cs_str(p), cs_str(items[p][0]), cs_str(items[p][1]))
        for p in sorted(items)]

src = '''using System;
using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// ชื่อ + ไอคอนของไอเทมแต่ละ prototype — **สร้างอัตโนมัติ อย่าแก้ด้วยมือ**
/// (`python scripts/extract_item_names.py "../game/DurangoV2_Data/resources.strings.txt" ServerCore/ItemNameData.cs`)
///
/// มาจาก TextAsset `prototype_data` — %d ชนิด
/// ใช้ตอนสร้างไอเทมที่ prototype ไม่ตรงกับชื่อสูตร (สูตรเดียวได้ผลลัพธ์หลายแบบตามวัตถุดิบ)
/// ชื่อเป็นภาษาเกาหลีเหมือนที่เกมใช้ (client แสดงตามที่ server ส่งไป)
/// </summary>
public static class ItemNameData
{
    public static readonly Dictionary<string, (string Name, string Icon)> Map =
        new Dictionary<string, (string, string)>(StringComparer.Ordinal)
    {
%s
    };

    /// <summary>ชื่อของ prototype นี้ (ไม่รู้จัก = คืน fallback ที่ส่งมา)</summary>
    public static string NameOf(string prototype, string fallback = null)
    {
        if (!string.IsNullOrEmpty(prototype) && Map.TryGetValue(prototype, out var row) && !string.IsNullOrEmpty(row.Name))
        {
            return row.Name;
        }
        return fallback ?? prototype;
    }

    /// <summary>ไอคอนของ prototype นี้</summary>
    public static string IconOf(string prototype, string fallback = null)
    {
        if (!string.IsNullOrEmpty(prototype) && Map.TryGetValue(prototype, out var row) && !string.IsNullOrEmpty(row.Icon))
        {
            return row.Icon;
        }
        return fallback ?? string.Empty;
    }
}
''' % (len(items), '\n'.join(rows))

OUT.write_text(src, encoding='utf-8')
print('เขียน %s แล้ว — ไอเทม %d ชนิด' % (OUT, len(items)))
