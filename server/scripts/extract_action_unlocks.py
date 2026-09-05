"""สกัด "สกิลไหนปลดล็อกท่าต่อสู้อะไร" จาก TextAsset `skills` + `rewards` → ActionUnlockData.cs

ใช้: python scripts/extract_action_unlocks.py <resources.strings.txt> <ServerCore/ActionUnlockData.cs>

ทำไมต้องมี: เดิม `HandleUseBattleAction` ตรวจแค่ tag อาวุธ ไม่เคยเช็ค `_knownSkills`
⇒ modded client ใช้ท่าต่อสู้ได้ทุกอย่างโดยไม่ต้องเรียนสกิล (เจ้าของย้ำ 2 รอบ:
"ท่าต่อสู้ก็ต้องยึดจากสกิลที่เรียน")

สายข้อมูลจริงในเกม (ไล่จาก client/Yaml/Reward.cs ที่มีฟิลด์ ActionIds + RewardType.Action=8):

    skills  : { "<หมวด>": { "<skillId>": { "<subId>": [ {rewards:[...], category_level:N}, ... ] } } }
              index ของ array = เลเวลของสกิลนั้น (0 = เลเวล 1)
    rewards : { "<rewardId>": { "action_ids": [...], "type": 8 } }
              type=8 (RewardType.Action) = ปลดล็อกท่าต่อสู้
"""
import io
import json
import re
import sys
import pathlib

SRC = pathlib.Path(sys.argv[1])
OUT = pathlib.Path(sys.argv[2])

lines = io.open(SRC, 'r', encoding='utf-8', errors='replace').read().split('\n')


def find_asset(name):
    for i, line in enumerate(lines):
        if line.rstrip() == name:
            return i
    raise SystemExit('หา TextAsset `%s` ไม่เจอ' % name)


def load_asset(name):
    """ตัว dump ตัดปีกกาชั้นนอกทิ้ง — เติมเองแล้วลองปิดทีละชั้นจนกว่าจะ parse ผ่าน"""
    at = find_asset(name)
    body = []
    for line in lines[at + 1:]:
        if line and not line[0].isspace():
            break
        body.append(line)
    text = '\n'.join(body)
    for extra in range(0, 6):
        try:
            return json.loads('{' + text + '}' * extra)
        except Exception:
            continue
    raise SystemExit('parse `%s` ไม่ผ่าน' % name)


skills = load_asset('skills')
rewards = load_asset('rewards')

# รายชื่อท่าต่อสู้ที่เซิร์ฟรู้จัก (จาก ActionData.cs — กันท่าที่มีในตาราง rewards แต่ไม่มีในเกมชุดนี้)
ad = (OUT.parent / 'ActionData.cs').read_text(encoding='utf-8')
known_actions = set(re.findall(r'A\("([^"]+)"', ad))


def reward_gives_actions(reward_id):
    """reward นี้ปลดล็อกท่าต่อสู้อะไรบ้าง (type=8 = Action)"""
    r = rewards.get(reward_id) or {}
    if r.get('type') != 8:
        return []
    return [x for x in (r.get('action_ids') or []) if x in known_actions]


# (skillId, subId) -> รายการต่อเลเวล: [ [action...], [action...], ... ]
by_skill = {}
unlocked_actions = set()

for _cat, skill_map in skills.items():
    if not isinstance(skill_map, dict):
        continue
    for skill_id, sub_map in skill_map.items():
        if not isinstance(sub_map, dict):
            continue
        for sub_id, entries in sub_map.items():
            if not isinstance(entries, list):
                continue
            per_level = []
            for entry in entries:
                act_here = []
                for rid in (entry.get('rewards') or []):
                    act_here += reward_gives_actions(rid)
                unlocked_actions.update(act_here)
                per_level.append(sorted(set(act_here)))
            if any(act for act in per_level):
                by_skill[(skill_id, sub_id)] = per_level

always_actions = sorted(known_actions - unlocked_actions)


def cs_arr(items):
    if not items:
        return 'new string[0]'
    return 'new[] { %s }' % ', '.join('"%s"' % x.replace('"', '') for x in items)


rows = []
for (skill_id, sub_id), per_level in sorted(by_skill.items()):
    lv_rows = []
    for acts in per_level:
        lv_rows.append(cs_arr(acts))
    rows.append('        { "%s|%s", new[]\n          {\n            %s\n          } },'
                % (skill_id, sub_id, ',\n            '.join(lv_rows)))

src = '''using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// **สกิลไหนปลดล็อกท่าต่อสู้อะไร** — ค่าจริงจากข้อมูลเกม
///
/// สกัดอัตโนมัติด้วย scripts/extract_action_unlocks.py - **อย่าแก้ด้วยมือ**
///
/// ใช้ที่ `HandleUseBattleAction` เพื่อตรวจว่าผู้เล่นเรียนสกิลที่ปลดล็อกท่านี้แล้วจริงไหม
/// เดิมตรวจแค่ tag อาวุธ ⇒ modded client ใช้ท่าได้ทุกอย่างโดยไม่เรียนสกิล
///
/// สรุปจำนวน: ท่าทั้งหมด %d - ปลดล็อกด้วยสกิล %d - **ได้ตั้งแต่แรก %d**
/// </summary>
public static class ActionUnlockData
{
    /// <summary>ท่าที่ไม่มีสกิลไหนปลดล็อก = ทุกคนได้ตั้งแต่เริ่ม (ท่าพื้นฐาน/หลบ/เทคลิ)</summary>
    public static readonly string[] AlwaysActions = %s;

    /// <summary>
    /// คีย์ <c>"skillId|subId"</c> → รายการท่าต่อเลเวล (index 0 = เลเวล 1)
    /// เรียนสกิลถึงเลเวล N = ได้ท่าจาก index 0 ถึง N-1 ทั้งหมด
    /// </summary>
    public static readonly Dictionary<string, string[][]> BySkill = new Dictionary<string, string[][]>
    {
%s
    };

    /// <summary>ท่าที่สกิลนี้ให้เมื่อเรียนถึงเลเวลที่กำหนด (สะสมตั้งแต่เลเวล 1)</summary>
    public static void Collect(string skillId, string subId, int level, HashSet<string> actions)
    {
        if (!BySkill.TryGetValue(skillId + "|" + subId, out string[][] levels))
        {
            return;
        }
        int upto = level < levels.Length ? level : levels.Length;
        for (int i = 0; i < upto; i++)
        {
            for (int j = 0; j < levels[i].Length; j++)
            {
                actions.Add(levels[i][j]);
            }
        }
    }
}
''' % (len(known_actions), len(unlocked_actions), len(always_actions),
       cs_arr(always_actions), '\n'.join(rows))

OUT.write_text(src, encoding='utf-8')
print('เขียน %s' % OUT.name)
print('  ท่าทั้งหมด %d - ปลดล็อกด้วยสกิล %d - ได้ตั้งแต่แรก %d'
      % (len(known_actions), len(unlocked_actions), len(always_actions)))
print('  สกิลที่ให้ท่า %d ตัว' % len(by_skill))
