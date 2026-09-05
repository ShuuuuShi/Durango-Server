"""สกัด "สกิลไหนปลดล็อกสูตรอะไร" จาก TextAsset `skills` + `rewards` → RecipeUnlockData.cs

ใช้: python scripts/extract_recipe_unlocks.py <resources.strings.txt> <ServerCore/RecipeUnlockData.cs>

ทำไมต้องมี: เดิม `GetRecipes` ส่งสูตรทั้ง 720 อันให้ทุกคน ⇒ **เลเวล 1 ไม่ได้เรียนสกิลอะไรเลย
ก็เห็นสูตรครบทุกอัน** ในเกมจริงเซิร์ฟส่งเฉพาะสูตรที่ปลดล็อกแล้ว

สายข้อมูลจริงในเกม (ไล่จาก client/Durango.Logic.Skill/Reward.cs ที่มีฟิลด์ RecipeIds):

    skills  : { "<หมวด>": { "<skillId>": { "<subId>": [ {rewards:[...], category_level:N}, ... ] } } }
              index ของ array = เลเวลของสกิลนั้น (0 = เลเวล 1)
    rewards : { "<rewardId>": { "recipe_ids": [...], "blueprint_ids": [...] } }

ตัวเลขที่นับได้จริง: สูตรทั้งหมด 720 - **ปลดล็อกด้วยสกิล 604** - **ไม่มีสกิลไหนปลดล็อก 116**
116 อันนั้นคือของที่ได้ตั้งแต่แรก (ไม่งั้นผู้เล่นใหม่คราฟอะไรไม่ได้เลย)
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

# รายชื่อสูตร/แปลนที่เซิร์ฟรู้จัก (กันสูตรที่มีในตารางแต่ไม่มีในเกมชุดนี้)
rd = (OUT.parent / 'RecipeData.cs').read_text(encoding='utf-8')
known_recipes = set(re.findall(r'"([^"]+)"', rd.split('AllRecipeIds = new[]', 1)[1].split('};', 1)[0]))
known_bps = set(re.findall(r'"([^"]+)"', rd.split('AllBlueprintIds = new[]', 1)[1].split('};', 1)[0]))


def reward_gives(reward_id):
    r = rewards.get(reward_id) or {}
    rec = [x for x in (r.get('recipe_ids') or []) if x in known_recipes]
    bps = [x for x in (r.get('blueprint_ids') or []) if x in known_bps]
    return rec, bps


# (skillId, subId) -> รายการต่อเลเวล: [ [recipe...], [recipe...], ... ]
by_skill = {}
unlocked_recipes = set()
unlocked_bps = set()

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
                rec_here, bp_here = [], []
                for rid in (entry.get('rewards') or []):
                    rec, bps = reward_gives(rid)
                    rec_here += rec
                    bp_here += bps
                unlocked_recipes.update(rec_here)
                unlocked_bps.update(bp_here)
                per_level.append((sorted(set(rec_here)), sorted(set(bp_here))))
            if any(r or b for r, b in per_level):
                by_skill[(skill_id, sub_id)] = per_level

always_recipes = sorted(known_recipes - unlocked_recipes)
always_bps = sorted(known_bps - unlocked_bps)


def arr(items):
    return ', '.join('"%s"' % x.replace('"', '') for x in items)


def cs_arr(items):
    return 'new string[0]' if not items else 'new[] { %s }' % arr(items)


rows = []
for (skill_id, sub_id), per_level in sorted(by_skill.items()):
    lv_rows = []
    for rec, bps in per_level:
        # `new[] { }` เปล่า ๆ คอมไพล์ไม่ผ่าน (อนุมานชนิดไม่ได้) ต้องเขียน new string[0]
        lv_rows.append('new Unlock(%s, %s)' % (cs_arr(rec), cs_arr(bps)))
    rows.append('        { "%s|%s", new[]\n          {\n            %s\n          } },'
                % (skill_id, sub_id, ',\n            '.join(lv_rows)))

src = '''using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>
/// **สกิลไหนปลดล็อกสูตร/แปลนอะไร** — ค่าจริงจากข้อมูลเกม
///
/// สกัดอัตโนมัติด้วย scripts/extract_recipe_unlocks.py - **อย่าแก้ด้วยมือ**
///
/// ใช้ที่ `GetRecipes` / `GetArtifactBlueprints` เพื่อส่งเฉพาะของที่ปลดล็อกแล้ว
/// เดิมส่งครบทั้ง %d สูตรให้ทุกคน ⇒ ไม่ได้เรียนสกิลอะไรเลยก็เห็นสูตรครบ
///
/// สรุปจำนวน: สูตรทั้งหมด %d - ปลดล็อกด้วยสกิล %d - **ได้ตั้งแต่แรก %d**
/// </summary>
public static class RecipeUnlockData
{
    public readonly struct Unlock
    {
        public readonly string[] Recipes;
        public readonly string[] Blueprints;

        public Unlock(string[] recipes, string[] blueprints)
        {
            Recipes = recipes;
            Blueprints = blueprints;
        }
    }

    /// <summary>สูตรที่ไม่มีสกิลไหนปลดล็อก = ทุกคนได้ตั้งแต่เริ่ม (ไม่งั้นผู้เล่นใหม่คราฟอะไรไม่ได้เลย)</summary>
    public static readonly string[] AlwaysRecipes = %s;

    /// <summary>แปลนที่ได้ตั้งแต่แรกด้วยเหตุผลเดียวกัน</summary>
    public static readonly string[] AlwaysBlueprints = %s;

    /// <summary>
    /// คีย์ <c>"skillId|subId"</c> → รายการต่อเลเวล (index 0 = เลเวล 1)
    /// เรียนสกิลถึงเลเวล N = ได้ของจาก index 0 ถึง N-1 ทั้งหมด
    /// </summary>
    public static readonly Dictionary<string, Unlock[]> BySkill = new Dictionary<string, Unlock[]>
    {
%s
    };

    /// <summary>ของที่สกิลนี้ให้เมื่อเรียนถึงเลเวลที่กำหนด (สะสมตั้งแต่เลเวล 1)</summary>
    public static void Collect(string skillId, string subId, int level, HashSet<string> recipes, HashSet<string> blueprints)
    {
        if (!BySkill.TryGetValue(skillId + "|" + subId, out Unlock[] levels))
        {
            return;
        }
        int upto = level < levels.Length ? level : levels.Length;
        for (int i = 0; i < upto; i++)
        {
            for (int j = 0; j < levels[i].Recipes.Length; j++)
            {
                recipes.Add(levels[i].Recipes[j]);
            }
            for (int j = 0; j < levels[i].Blueprints.Length; j++)
            {
                blueprints.Add(levels[i].Blueprints[j]);
            }
        }
    }
}
''' % (len(known_recipes), len(known_recipes), len(unlocked_recipes), len(always_recipes),
       cs_arr(always_recipes), cs_arr(always_bps), '\n'.join(rows))

OUT.write_text(src, encoding='utf-8')
print('เขียน %s' % OUT.name)
print('  สูตรทั้งหมด %d - ปลดล็อกด้วยสกิล %d - ได้ตั้งแต่แรก %d'
      % (len(known_recipes), len(unlocked_recipes), len(always_recipes)))
print('  แปลนทั้งหมด %d - ปลดล็อกด้วยสกิล %d - ได้ตั้งแต่แรก %d'
      % (len(known_bps), len(unlocked_bps), len(always_bps)))
print('  สกิลที่ให้ของ %d ตัว' % len(by_skill))
