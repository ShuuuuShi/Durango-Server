"""สกัดท่าโจมตีของผู้เล่น (TextAsset `player_battle_actions` + `tag_allow_actions`) → ActionData.cs

ใช้: python scripts/extract_actions.py <resources.strings.txt> <ServerCore/ActionData.cs>

`player_battle_actions`: id ของท่า → attack_info[] + meta (stamina/cooltime/use_range/motion)
`tag_allow_actions`: tag ของอาวุธ (เช่น bare_hands, axe_onehand) → default_actions / skill_actions
"""
import io
import json
import sys
import pathlib

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])

lines = io.open(SRC, 'r', encoding='utf-8', errors='replace').read().split('\n')


def asset_block(name):
    """คืน dict ของ TextAsset ชื่อนี้ (dump ตัดปีกกาชั้นนอกทิ้ง จึงต้องเติมกลับ)"""
    start = None
    for i, line in enumerate(lines):
        if line.rstrip() == name:
            start = i + 1
            break
    if start is None:
        raise SystemExit('หา TextAsset `%s` ไม่เจอ' % name)
    body = []
    for line in lines[start:]:
        if line and not line[0].isspace():
            break
        body.append(line)
    text = '\n'.join(body).rstrip().rstrip(',')
    last = None
    for extra in range(1, 5):
        try:
            return json.loads('{' + text + '}' * extra)
        except Exception as e:      # noqa: BLE001
            last = e
    raise SystemExit('parse `%s` ไม่ได้: %s' % (name, last))


actions = asset_block('player_battle_actions')
tag_actions = asset_block('tag_allow_actions')


def num(v, default=0.0):
    try:
        return float(v)
    except (TypeError, ValueError):
        return default


rows = []
for action_id in sorted(actions):
    a = actions[action_id] or {}
    meta = a.get('meta') or {}
    infos = a.get('attack_info') or []
    info = infos[0] if infos else {}
    ratio = info.get('atk_ratio') or {}
    rows.append((
        action_id,
        int(num(meta.get('stamina'))),
        num(meta.get('cooltime')),
        num(meta.get('use_range'), 200.0),
        num(info.get('attack_time'), 0.6),
        num(info.get('damage_bonus'), 1.0),
        num(ratio.get('impact')),
        num(ratio.get('pierce')),
        num(ratio.get('cut')),
        num(info.get('radius'), 200.0),
        bool(info.get('strong_attack')),
    ))

weapon_rows = []
for tag in sorted(tag_actions):
    entry = tag_actions[tag] or {}
    ids = list(entry.get('default_actions') or []) + list(entry.get('skill_actions') or [])
    ids = [i for i in ids if i in actions]
    if ids:
        weapon_rows.append((tag, ids))


def f(v):
    return ('%gf' % v)


out = []
out.append('using System.Collections.Generic;')
out.append('')
out.append('namespace DurangoServer.Core;')
out.append('')
out.append('// เฟส C รอบ 2: ท่าโจมตีของผู้เล่น (generated จาก resources.strings.txt)')
out.append('// สร้างด้วย scripts/extract_actions.py — อย่าแก้มือ')
out.append('//')
out.append('// Actions[id]        = ค่าของท่านั้น (สตามินา/คูลดาวน์/ระยะ/เวลาที่ดาเมจเข้า/ตัวคูณดาเมจ)')
out.append('// WeaponActions[tag] = ท่าที่ใช้ได้เมื่อถืออาวุธที่มี tag นั้น (bare_hands = มือเปล่า)')
out.append('public static class ActionData')
out.append('{')
out.append('    public sealed class Action')
out.append('    {')
out.append('        public readonly string Id;')
out.append('        /// <summary>สตามินาที่ใช้ต่อครั้ง</summary>')
out.append('        public readonly int Stamina;')
out.append('        /// <summary>คูลดาวน์ (วินาที)</summary>')
out.append('        public readonly float Cooltime;')
out.append('        /// <summary>ระยะที่ใช้ท่านี้ได้ (หน่วยโลก; 1 tile = 200)</summary>')
out.append('        public readonly float UseRange;')
out.append('        /// <summary>ดาเมจเข้าหลังกดกี่วินาที</summary>')
out.append('        public readonly float AttackTime;')
out.append('        /// <summary>ตัวคูณดาเมจของท่า</summary>')
out.append('        public readonly float DamageBonus;')
out.append('        public readonly float Impact;')
out.append('        public readonly float Pierce;')
out.append('        public readonly float Cut;')
out.append('        public readonly float Radius;')
out.append('        public readonly bool Strong;')
out.append('')
out.append('        public Action(string id, int stamina, float cooltime, float useRange, float attackTime,')
out.append('            float damageBonus, float impact, float pierce, float cut, float radius, bool strong)')
out.append('        {')
out.append('            Id = id; Stamina = stamina; Cooltime = cooltime; UseRange = useRange;')
out.append('            AttackTime = attackTime; DamageBonus = damageBonus;')
out.append('            Impact = impact; Pierce = pierce; Cut = cut; Radius = radius; Strong = strong;')
out.append('        }')
out.append('')
out.append('        /// <summary>ผลรวมสัดส่วนดาเมจของท่า (ใช้เป็นตัวคูณรวมกับ DamageBonus)</summary>')
out.append('        public float RatioSum => Impact + Pierce + Cut;')
out.append('    }')
out.append('')
out.append('    private static Action A(string id, int stamina, float cooltime, float useRange, float attackTime,')
out.append('        float damageBonus, float impact, float pierce, float cut, float radius, bool strong)')
out.append('    {')
out.append('        return new Action(id, stamina, cooltime, useRange, attackTime, damageBonus, impact, pierce, cut, radius, strong);')
out.append('    }')
out.append('')
out.append('    public static readonly Dictionary<string, Action> Actions = new Dictionary<string, Action>()')
out.append('    {')
for r in rows:
    out.append('        { "%s", A("%s", %d, %s, %s, %s, %s, %s, %s, %s, %s, %s) },' % (
        r[0], r[0], r[1], f(r[2]), f(r[3]), f(r[4]), f(r[5]), f(r[6]), f(r[7]), f(r[8]), f(r[9]),
        'true' if r[10] else 'false'))
out.append('    };')
out.append('')
out.append('    public static readonly Dictionary<string, string[]> WeaponActions = new Dictionary<string, string[]>()')
out.append('    {')
for tag, ids in weapon_rows:
    out.append('        { "%s", new[] { %s } },' % (tag, ', '.join('"%s"' % i for i in ids)))
out.append('    };')
out.append('')
out.append('    public static bool TryGet(string actionId, out Action action)')
out.append('    {')
out.append('        action = null;')
out.append('        return !string.IsNullOrEmpty(actionId) && Actions.TryGetValue(actionId, out action);')
out.append('    }')
out.append('')
out.append('    /// <summary>ท่าที่ใช้ได้ของอาวุธ tag นี้ (ไม่รู้จัก = มือเปล่า)</summary>')
out.append('    public static string[] ForWeaponTag(string tag)')
out.append('    {')
out.append('        if (!string.IsNullOrEmpty(tag) && WeaponActions.TryGetValue(tag, out string[] ids))')
out.append('        {')
out.append('            return ids;')
out.append('        }')
out.append('        return WeaponActions.TryGetValue("bare_hands", out string[] bare) ? bare : new string[0];')
out.append('    }')
out.append('}')
out.append('')

OUT.write_text('\n'.join(out), encoding='utf-8')
print('เขียน %s: ท่า %d ท่า, อาวุธ %d แบบ' % (OUT, len(rows), len(weapon_rows)))
