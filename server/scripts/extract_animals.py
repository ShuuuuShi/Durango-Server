"""สกัดข้อมูลสัตว์ (entity type 2000-2999) จาก resources.strings.txt → AnimalData.cs"""
import json, re, sys, pathlib

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])
text = SRC.read_text(encoding='utf-8', errors='replace')
lines = text.split('\n')
_off = [0]
for _l in lines:
    _off.append(_off[-1] + len(_l) + 1)


def block_at(line_idx):
    start = text.index('{', _off[line_idx])
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
            elif c == '{':
                depth += 1
            elif c == '}':
                depth -= 1
                if depth == 0:
                    return text[start:i + 1]
        i += 1
    return None


animals = {}
pat = re.compile(r'^\s*"(2\d{3})":\s*\{\s*$')
for idx, ln in enumerate(lines):
    m = pat.match(ln)
    if not m:
        continue
    raw = block_at(idx)
    if raw is None:
        continue
    try:
        obj = json.loads(raw)
    except Exception:
        continue
    # กรองเฉพาะ block ที่หน้าตาเป็นสัตว์จริง
    if 'model_path' not in obj or 'life_max' not in obj:
        continue
    animals.setdefault(int(m.group(1)), obj)

print('เจอสัตว์ %d ชนิด' % len(animals))


def esc(s):
    if s is None:
        return 'null'
    return '"' + str(s).replace(chr(92), chr(92) * 2).replace('"', chr(92) + '"') + '"'


def first_name(obj):
    """name เป็น dict { "ชื่อเกาหลี": null } — เอา key แรก"""
    n = obj.get('name')
    if isinstance(n, dict) and n:
        return list(n.keys())[0]
    if isinstance(n, str):
        return n
    return None


def num(v, default=0.0):
    """ค่าบางตัวเป็นสูตรข้อความ เช่น '(0 + combat_level * 5)' — ใช้ default แทน"""
    if isinstance(v, (int, float)):
        return float(v)
    return default


rows = []
for et in sorted(animals):
    a = animals[et]
    name = first_name(a)
    model = a.get('model_path')
    if not model:
        continue
    rows.append('        { %d, new AnimalInfo(%d, %s, %s, %sf, %s, %s, %d, %sf) },' % (
        et, et, esc(name), esc(model),
        num(a.get('represent_scale'), 1.0),
        esc(a.get('ai_factor_id')),
        'true' if a.get('tamable') else 'false',
        int(num(a.get('size_level'), 1.0)) or 1,
        num(a.get('difficulty'), 1.0)))

src = '''using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// เฟส C — ข้อมูลสัตว์ (entity type 2000–2999)
///
/// สกัดอัตโนมัติจาก game/DurangoV2_Data/resources.strings.txt ด้วย scripts/extract_animals.py
/// **อย่าแก้ด้วยมือ** ให้รันสคริปต์ใหม่แทน
///
/// เก็บเฉพาะที่ server ต้องใช้ตอน spawn — ค่าพลังชีวิต/ดาเมจจริงในเกมเป็น "สูตรข้อความ"
/// (เช่น "(0 + combat_level * 5) * unstable_factor") ที่ต้องมี NCalc มาคำนวณ
/// ตอนนี้ยังไม่ได้ทำ จึงใช้ค่าคงที่จาก AnimalSpawner แทน
/// </summary>
public static class AnimalData
{
    public readonly struct AnimalInfo
    {
        public readonly ushort EntityType;
        public readonly string Name;
        public readonly string ModelPath;
        public readonly float Scale;
        public readonly string AiFactorId;
        public readonly bool Tamable;

        /// <summary>
        /// ขนาดตัวจริงจากข้อมูลเกม (size_level 1–7)
        /// 1 = กิ้งก่า/คอมป์โซ · 2 = แร็ปเตอร์/โปรโตเซราท็อปส์ · 4 = สเตโก/ทริเซรา · 7 = ตัวใหญ่สุด
        /// **ไม่ใช่ Scale** — Scale เป็นตัวคูณของ prefab แต่ละโมเดล เทียบข้ามชนิดไม่ได้
        /// (แร็ปเตอร์ Scale 2.2 แต่ตัวเล็กกว่าบราคิโอที่ Scale 1.27)
        /// server ใช้ค่านี้กำหนดว่าตัวใหญ่ต้องเกิดลึกเข้าไปในเกาะแค่ไหน
        /// </summary>
        public readonly int SizeLevel;

        /// <summary>ความยากจากข้อมูลเกม (0.3 = กิ้งก่า … 10 = ทริเซราท็อปส์)</summary>
        public readonly float Difficulty;

        public AnimalInfo(int entityType, string name, string modelPath, float scale, string aiFactorId, bool tamable,
            int sizeLevel, float difficulty)
        {
            EntityType = (ushort)entityType;
            Name = name;
            ModelPath = modelPath;
            Scale = scale;
            AiFactorId = aiFactorId;
            Tamable = tamable;
            SizeLevel = sizeLevel;
            Difficulty = difficulty;
        }
    }

    /// <summary>สัตว์ %d ชนิด</summary>
    public static readonly Dictionary<ushort, AnimalInfo> All = new Dictionary<ushort, AnimalInfo>
    {
%s
    };

    public static bool TryGet(ushort entityType, out AnimalInfo info)
    {
        return All.TryGetValue(entityType, out info);
    }
}
''' % (len(rows), '\n'.join(rows))

OUT.write_text(src, encoding='utf-8')
print('เขียน %s — %d ชนิด' % (OUT.name, len(rows)))
