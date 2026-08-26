"""สกัดข้อมูลพืชผล + ปุ๋ย + ภาชนะตักน้ำ จาก resources.strings.txt → CropData.cs

ข้อมูลมาจาก 4 ตารางในไฟล์เดียวกัน:
  crops                  — เนื้อในของระบบปลูก (เวลาโต · น้ำ · ปุ๋ย · ไบโอมที่ชอบ · ได้อะไรตอนเก็บ)
  crop_data              — ชื่อ/ไอคอน/เลเวลสูงสุดของ "เมล็ด"
  generator_client_data  — ชื่อ/ไอคอนของ "ผลผลิต" (corn_crop, corn_seed_crop)
  performance            — ปุ๋ยที่ไอเทมให้ (`fertilizer`) + ความจุภาชนะ (`container.capacity`)

⚠️ asset `performance` มี **ก้อน asset ดิบแทรกกลาง** (color_cloth.raw ฯลฯ) ที่บรรทัดชิดซ้าย
   ⇒ กฎ "เจอบรรทัดชิดซ้าย = จบ asset" ตัดกลางคัน ตาราง fertilizer ท้าย ๆ หายไปทั้งก้อน
   ต้องอ่านตารางย่อยด้วยการนับปีกกาแทน (`read_subtable`)

⚠️ ข้อมูลเกม **ไม่มี tag ของผลผลิต** (corn_crop โผล่ทั้งไฟล์แค่ 3 ที่
   คือ crops · collectible_names · generator_client_data)
   ⇒ สคริปต์เดา tag ของผลผลิตจาก **tag ของเมล็ด** โดยตัด "plantable" ออก
   (corn_seed = eatable/grain/plantable/vegetable ⇒ corn_crop = eatable/grain/vegetable)
   เหตุผลเดียวกับที่ ItemTagData เดา tag ชนิดเครื่องมือจากชื่อ prototype

`grows_until` เป็นสูตรของเลเวลเมล็ด เช่น "(60 * level + 600) * 0.5" (ส่วนใหญ่เป็นค่าคงที่)
เก็บเป็น (ค่าที่เลเวล 1, ค่าที่เพิ่มต่อ 1 เลเวล) แบบเดียวกับ EquipData/FoodData

ใช้: python scripts/extract_crops.py <resources.strings.txt> <ServerCore/CropData.cs> <ServerCore/ItemTagData.cs>
"""
import json
import re
import sys
import pathlib

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])
TAGSRC = pathlib.Path(sys.argv[3]) if len(sys.argv) > 3 else None

QUOTE = chr(34)
BACKSLASH = chr(92)
NL = chr(10)
text = SRC.read_text(encoding='utf-8', errors='replace')


def _close_and_parse(body):
    """ตารางท้าย ๆ ในไฟล์นี้ 'ปีกกาปิดหาย' — เติมที่ขาดให้ครบก่อน parse"""
    depth_c = depth_b = 0
    in_str = esc = False
    for ch in body:
        if in_str:
            if esc:
                esc = False
            elif ch == BACKSLASH:
                esc = True
            elif ch == QUOTE:
                in_str = False
            continue
        if ch == QUOTE:
            in_str = True
        elif ch == '{':
            depth_c += 1
        elif ch == '}':
            depth_c -= 1
        elif ch == '[':
            depth_b += 1
        elif ch == ']':
            depth_b -= 1
    body += ']' * max(0, depth_b) + '}' * max(0, depth_c)
    return json.loads('{' + body + '}')


def _slice_at(start):
    """ตัดเนื้อในของ asset ที่เริ่มที่ offset นี้ จนถึงบรรทัดชื่อ asset ถัดไป"""
    lines = text[start:].split(NL)
    end = len(lines)
    for k, ln in enumerate(lines):
        if ln and not ln.startswith(' ') and not ln.startswith('}') and not ln.startswith('{'):
            end = k
            break
    return NL.join(lines[:end]).strip().rstrip(',')


def read_table(name):
    """อ่าน asset ก้อนแรกที่ชื่อ name (ชื่ออยู่บนบรรทัดของตัวเองแบบไม่มีเว้นวรรคนำ)"""
    key = NL + name + NL
    i = text.index(key)
    return _close_and_parse(_slice_at(i + len(key)))


def read_subtable(name):
    """อ่านตารางย่อย `  "<name>": { ... }` ด้วยการนับปีกกา

    ใช้แทน _slice_at กับ asset `performance` เพราะในไฟล์มี **ก้อน asset ดิบ** (เช่น color_cloth.raw)
    แทรกกลางอยู่ — บรรทัดพวกนั้นไม่มีเว้นวรรคนำ กฎ "เจอบรรทัดชิดซ้าย = จบ asset" เลยตัดกลางคัน
    ทำให้ตาราง fertilizer ที่อยู่ท้าย ๆ ของ asset หายไปทั้งก้อน
    """
    key = NL + '  ' + QUOTE + name + QUOTE + ': {' + NL
    start = 0
    while True:
        i = text.find(key, start)
        if i < 0:
            return {}
        open_at = i + len(key) - 2          # ตำแหน่งของ '{'
        depth = 0
        in_str = esc = False
        end = None
        for j in range(open_at, min(len(text), open_at + 4000000)):
            ch = text[j]
            if in_str:
                if esc:
                    esc = False
                elif ch == BACKSLASH:
                    esc = True
                elif ch == QUOTE:
                    in_str = False
                continue
            if ch == QUOTE:
                in_str = True
            elif ch == '{':
                depth += 1
            elif ch == '}':
                depth -= 1
                if depth == 0:
                    end = j + 1
                    break
        start = i + len(key)
        if end is None:
            continue
        try:
            part = json.loads(text[open_at:end])
        except Exception:                   # noqa: BLE001
            continue
        if part:
            return part


crops = read_table('crops')
crop_data = read_table('crop_data')
gen_names = read_table('generator_client_data')
fert_table = read_subtable('fertilizer')
cont_table = read_subtable('container')

# ---------------------------------------------------------------- tag ของเมล็ด
seed_tags = {}
if TAGSRC and TAGSRC.exists():
    tagtext = TAGSRC.read_text(encoding='utf-8')
    row_pat = '{ ' + QUOTE + '([a-z0-9_]+)' + QUOTE + ', new' + BACKSLASH + '[' + BACKSLASH + '] {([^}]*)} }'
    tag_pat = 'T' + BACKSLASH + '(' + QUOTE + '([a-z0-9_]+)' + QUOTE + ',(' + BACKSLASH + 'd+)' + BACKSLASH + ')'
    for m in re.finditer(row_pat, tagtext):
        proto, body = m.group(1), m.group(2)
        tags = re.findall(tag_pat, body)
        if tags:
            seed_tags[proto] = tags

BIOME = {
    'temperate_forest': 'TemperateForest',
    'tropical_forest': 'TropicalForest',
    'desert': 'Desert',
    'tundra': 'Tundra',
    'grassland': 'Grassland',
}

SAFE_EXPR = re.compile('^[0-9level' + BACKSLASH + 's.+' + BACKSLASH + '-*/()]*$')


def range_lookup(pairs, level):
    """สูตรแบบขั้นบันไดของเกม — เอาค่าของช่วงที่เลเวลตกอยู่"""
    out = pairs[0][1]
    for lv, val in pairs:
        if level >= lv:
            out = val
    return float(out)


def ev(expr, level):
    """ประเมินสูตรของเลเวล — มีแค่เลขคณิตกับ range_lookup"""
    if expr is None:
        return 0.0
    s = str(expr).strip()
    if s == '':
        return 0.0
    try:
        return float(s)
    except ValueError:
        pass
    if 'range_lookup' not in s and not SAFE_EXPR.match(s):
        raise ValueError('สูตรมีอย่างอื่นนอกจากเลขคณิต: ' + s)
    env = {'level': level, 'range_lookup': range_lookup}
    return float(eval(s, {'__builtins__': {}}, env))       # noqa: S307 — กรองด้วย SAFE_EXPR แล้ว


def first_name(d):
    if isinstance(d, dict) and d:
        return list(d.keys())[0].strip()
    return None


def cs(s):
    if s is None:
        return 'null'
    return QUOTE + s.replace(BACKSLASH, BACKSLASH * 2).replace(QUOTE, BACKSLASH + QUOTE) + QUOTE


rows = []
missing_seed_product = []
for seed_id in sorted(crops.keys()):
    variants = crops[seed_id]
    info = variants.get('__default__') or list(variants.values())[0]
    meta = crop_data.get(seed_id, {})

    grow1 = ev(info.get('grows_until'), 1)
    grow2 = ev(info.get('grows_until'), 2)
    per_level = grow2 - grow1

    product = info.get('grows_to')
    seed_product = seed_id + '_crop'
    if seed_product not in gen_names:
        seed_product = None
        missing_seed_product.append(seed_id)

    pname = first_name((gen_names.get(product) or {}).get('name')) if product else None
    picon = (gen_names.get(product) or {}).get('icon') if product else None
    spname = first_name((gen_names.get(seed_product) or {}).get('name')) if seed_product else None
    spicon = (gen_names.get(seed_product) or {}).get('icon') if seed_product else None

    tags = [t for t in seed_tags.get(seed_id, []) if t[0] != 'plantable']
    if not tags:
        # เมล็ดบางชนิดมี tag แค่ "plantable" ตัวเดียว ⇒ เดาจากไอคอนของผลผลิตแทน
        # เอาเฉพาะที่ไอคอนบอกชัด ที่เหลือปล่อยว่างไว้ดีกว่าเดามั่ว
        ic = picon or ''
        if '_fruit_' in ic or ic.endswith(('_grape', '_watermelon_01')):
            tags = [('eatable', '1'), ('fruit', '1')]
        elif ic.endswith(('_chilipepper', '_onion')):
            tags = [('eatable', '1'), ('vegetable', '1')]
        elif ic.endswith('_mushroom'):
            tags = [('eatable', '1'), ('mushroom', '1')]
        elif ic.endswith(('_rose', '_sunflower')):
            tags = [('flower', '1')]
        elif '_fiber_' in ic or ic.endswith('_cotton'):
            tags = [('fiber', '1')]
    tag_src = 'null' if not tags else 'new[] { ' + ', '.join(
        'new Tag { Id = ' + cs(t[0]) + ', Level = ' + t[1] + ' }' for t in tags) + ' }'

    looks = info.get('grown_looks') or []
    looks_src = 'null' if not looks else 'new[] { ' + ', '.join(cs(x) for x in looks) + ' }'
    look = info.get('look') or {}

    pref = BIOME.get(info.get('preference_land') or '', 'Invalid')

    rows.append(
        '        { ' + cs(seed_id) + ', new CropInfo' + NL
        + '          {' + NL
        + '              SeedId = ' + cs(seed_id) + ',' + NL
        + '              Name = ' + cs(first_name(meta.get('name'))) + ',' + NL
        + '              Icon = ' + cs(meta.get('icon')) + ',' + NL
        + '              MaxLevel = ' + str(meta.get('max_level') or 1) + ',' + NL
        + '              ProductId = ' + cs(product) + ',' + NL
        + '              ProductName = ' + cs(pname) + ',' + NL
        + '              ProductIcon = ' + cs(picon) + ',' + NL
        + '              ProductTags = ' + tag_src + ',' + NL
        + '              SeedProductId = ' + cs(seed_product) + ',' + NL
        + '              SeedProductName = ' + cs(spname) + ',' + NL
        + '              SeedProductIcon = ' + cs(spicon) + ',' + NL
        + '              GrowBase = ' + ('%.1ff' % grow1) + ',' + NL
        + '              GrowPerLevel = ' + ('%.1ff' % per_level) + ',' + NL
        + '              RequiredWater = ' + str(int(info.get('required_water') or 0)) + ',' + NL
        + '              RequiredFertilizer = ' + str(int(info.get('required_fertilizer') or 0)) + ',' + NL
        + '              AdditionalProduct = ' + str(int(info.get('additional_product') or 0)) + ',' + NL
        + '              Survivability = ' + ('%.2ff' % float(info.get('survivability') or 1.0)) + ',' + NL
        + '              Preference = Shared.Region.Biome.' + pref + ',' + NL
        + '              GrownLooks = ' + looks_src + ',' + NL
        + '              GrowingLook = ' + cs(look.get('growing')) + ',' + NL
        + '              DeadLook = ' + cs(look.get('dead')) + NL
        + '          } },'
    )


# ---------------------------------------------------------------- ปุ๋ย + ภาชนะตักน้ำ
def linear_rows(table, field):
    """<proto> -> (ค่าที่เลเวล 1, ค่าที่เพิ่มต่อเลเวล) — ข้ามตัวที่ค่าเป็น 0 ทั้งคู่"""
    out = []
    for proto in sorted(table.keys()):
        entry = table[proto]
        if not isinstance(entry, dict) or not entry:
            continue
        inner = list(entry.values())[0]
        if not isinstance(inner, dict):
            continue
        expr = inner.get(field)
        if expr is None:
            continue
        try:
            a = ev(expr, 1)
            b = ev(expr, 2)
        except Exception:                       # noqa: BLE001
            continue
        if a <= 0 and b <= 0:
            continue
        out.append((proto, a, b - a))
    return out


fert_rows = linear_rows(fert_table, 'fertilizer')
cont_rows = linear_rows(cont_table, 'capacity')


def pair_block(pairs):
    return NL.join('        { ' + cs(pid) + ', (%.2ff, %.4ff) },' % (a, per) for pid, a, per in pairs)


header = '''using System.Collections.Generic;
using Messages;

namespace DurangoServer.Core;

/// <summary>
/// ข้อมูลพืชผล + ปุ๋ย + ภาชนะตักน้ำ — **สร้างอัตโนมัติ อย่าแก้ด้วยมือ**
/// (`python scripts/extract_crops.py "../game/DurangoV2_Data/resources.strings.txt" ServerCore/CropData.cs ServerCore/ItemTagData.cs`)
///
/// · <see cref="CropInfo.GrowBase"/> เป็น **วินาที** ตามเกมจริง (ยาวถึง 24 ชม.)
///   เซิร์ฟย่อด้วย <c>Farming.GrowthScale</c> ใน config.json อีกที
/// · <see cref="CropInfo.ProductTags"/> เดาจาก tag ของเมล็ด (ข้อมูลเกมไม่มี tag ของผลผลิต)
/// </summary>
public static class CropData
{
    public struct CropInfo
    {
        /// <summary>prototype ของเมล็ด เช่น corn_seed</summary>
        public string SeedId;
        public string Name;
        public string Icon;
        public int MaxLevel;

        /// <summary>ผลผลิตหลักตอนเก็บเกี่ยว เช่น corn_crop</summary>
        public string ProductId;
        public string ProductName;
        public string ProductIcon;
        /// <summary>tag ของผลผลิต — เดาจาก tag ของเมล็ด (ตัด plantable ออก)</summary>
        public Tag[] ProductTags;

        /// <summary>เมล็ดที่ได้คืนตอนเก็บเกี่ยว เช่น corn_seed_crop (null = พืชชนิดนี้ไม่คืนเมล็ด)</summary>
        public string SeedProductId;
        public string SeedProductName;
        public string SeedProductIcon;

        /// <summary>เวลาโตเป็นวินาทีที่เมล็ดเลเวล 1</summary>
        public float GrowBase;
        public float GrowPerLevel;

        public int RequiredWater;
        public int RequiredFertilizer;
        /// <summary>ผลผลิตที่ได้เพิ่มเมื่อใส่ปุ๋ยเต็ม (นอกเหนือจาก 1 ชิ้นพื้นฐาน)</summary>
        public int AdditionalProduct;
        /// <summary>ความทนของพืช — ใช้เป็นเกณฑ์ว่ารดน้ำไม่ครบแค่ไหนถึงจะตาย</summary>
        public float Survivability;

        /// <summary>ไบโอมที่พืชชนิดนี้ชอบ (Invalid = ปลูกที่ไหนก็ได้)</summary>
        public Shared.Region.Biome Preference;

        public string[] GrownLooks;
        public string GrowingLook;
        public string DeadLook;

        /// <summary>เวลาโตของเมล็ดเลเวลนี้ (ยังไม่คูณ GrowthScale)</summary>
        public float GrowSecondsAt(int level)
        {
            float v = GrowBase + GrowPerLevel * (level - 1);
            return v < 1f ? 1f : v;
        }

        /// <summary>หน้าตาของต้นที่โตแล้ว — เลือกจากพิกัดให้แปลงเดิมได้ต้นเดิมเสมอ</summary>
        public string GrownLookFor(int tileX, int tileY)
        {
            if (GrownLooks == null || GrownLooks.Length == 0)
            {
                return null;
            }
            int h = (tileX * 73856093) ^ (tileY * 19349663);
            if (h < 0)
            {
                h = -h;
            }
            return GrownLooks[h % GrownLooks.Length];
        }
    }

    public static readonly Dictionary<string, CropInfo> ById = new Dictionary<string, CropInfo>
    {
'''

footer = '''    };

    public static bool TryGet(string seedPrototype, out CropInfo info)
    {
        return ById.TryGetValue(seedPrototype ?? string.Empty, out info);
    }

    /// <summary>เมล็ดชนิดนี้ปลูกได้ไหม</summary>
    public static bool IsSeed(string seedPrototype)
    {
        return !string.IsNullOrEmpty(seedPrototype) && ById.ContainsKey(seedPrototype);
    }

    // ---------------------------------------------------------------- ปุ๋ย
    /// <summary>ไอเทมชิ้นนี้ให้ปุ๋ยกี่หน่วย — (ค่าที่เลเวล 1, เพิ่มต่อ 1 เลเวล)</summary>
    public static readonly Dictionary<string, (float Base, float PerLevel)> FertilizerPower =
        new Dictionary<string, (float, float)>
    {
__FERT__
    };

    /// <summary>ปุ๋ยที่ได้จากไอเทมชิ้นนี้ที่เลเวลนี้ (0 = ไม่ใช่ปุ๋ย)</summary>
    public static float FertilizerOf(string prototype, int level)
    {
        if (string.IsNullOrEmpty(prototype) || !FertilizerPower.TryGetValue(prototype, out var v))
        {
            return 0f;
        }
        float amount = v.Base + v.PerLevel * (level - 1);
        return amount < 0f ? 0f : amount;
    }

    // ---------------------------------------------------------------- ภาชนะตักน้ำ
    /// <summary>ภาชนะใบนี้ตักน้ำได้กี่หน่วย — (ค่าที่เลเวล 1, เพิ่มต่อ 1 เลเวล)</summary>
    public static readonly Dictionary<string, (float Base, float PerLevel)> ContainerCapacity =
        new Dictionary<string, (float, float)>
    {
__CONT__
    };

    /// <summary>ภาชนะใบนี้ตักน้ำได้กี่หน่วยที่เลเวลนี้ (0 = ไม่ใช่ภาชนะ)</summary>
    public static float CapacityOf(string prototype, int level)
    {
        if (string.IsNullOrEmpty(prototype) || !ContainerCapacity.TryGetValue(prototype, out var v))
        {
            return 0f;
        }
        float amount = v.Base + v.PerLevel * (level - 1);
        return amount < 0f ? 0f : amount;
    }
}
'''

body = footer.replace('__FERT__', pair_block(fert_rows)).replace('__CONT__', pair_block(cont_rows))
OUT.write_text(header + NL.join(rows) + NL + body, encoding='utf-8')
print('เขียน %s แล้ว — พืช %d ชนิด · ปุ๋ย %d ชนิด · ภาชนะ %d ชนิด'
      % (OUT, len(rows), len(fert_rows), len(cont_rows)))
if missing_seed_product:
    print('พืชที่ไม่คืนเมล็ดตอนเก็บ (%d): %s' % (len(missing_seed_product), ', '.join(missing_seed_product)))
