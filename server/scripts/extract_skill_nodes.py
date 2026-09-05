"""Generate authoritative skill node data from the client's skills TextAsset dump."""
import json
import pathlib
import sys

src = pathlib.Path(sys.argv[1])
out = pathlib.Path(sys.argv[2])
lines = src.read_text(encoding="utf-8", errors="replace").splitlines()
start = None
for i, line in enumerate(lines):
    if line == "skills" and i + 1 < len(lines) and lines[i + 1].strip().startswith('"7"'):
        start = i + 1
        break
if start is None:
    raise SystemExit("skills asset not found")
body = []
for line in lines[start:]:
    if line and not line[0].isspace():
        break
    body.append(line)
blob = "\n".join(body).rstrip().rstrip(",")
data = None
for extra in range(1, 6):
    try:
        data = json.loads("{" + blob + "}" * extra)
        break
    except Exception:
        pass
if data is None:
    raise SystemExit("skills JSON could not be parsed")

def cs(value):
    return '"' + str(value).replace('\\', '\\\\').replace('"', '\\"') + '"'

rows = []
count = 0
for category, skills in data.items():
    for skill_id, subs in skills.items():
        for sub_id, nodes in subs.items():
            for level, node in enumerate(nodes, 1):
                rewards = node.get("rewards") or []
                reward_text = "new[] { " + ", ".join(cs(x) for x in rewards) + " }" if rewards else "Array.Empty<string>()"
                rows.append(
                    f"        {{ Key({cs(skill_id)}, {cs(sub_id)}, {level}), "
                    f"new Node((Shared.Skill.Category){int(category)}, {level}, {int(node.get('category_level') or 0)}, "
                    f"{int(node.get('skill_point') or 0)}, {(str(node.get('untrain_disabled')).lower())}, {reward_text}) }},"
                )
                count += 1

text = """using System;
using System.Collections.Generic;

namespace DurangoServer.Core;

/// <summary>Authoritative skill node costs and gates generated from the client skills asset.</summary>
public static class SkillNodeData
{
    public sealed class Node
    {
        public readonly Shared.Skill.Category Category;
        public readonly int Level;
        public readonly int CategoryLevel;
        public readonly int SkillPoint;
        public readonly bool UntrainDisabled;
        public readonly string[] Rewards;

        public Node(Shared.Skill.Category category, int level, int categoryLevel, int skillPoint,
            bool untrainDisabled, string[] rewards)
        {
            Category = category;
            Level = level;
            CategoryLevel = categoryLevel;
            SkillPoint = skillPoint;
            UntrainDisabled = untrainDisabled;
            Rewards = rewards;
        }
    }

    private static string Key(string skillId, string subId, int level) =>
        (skillId ?? string.Empty) + "|" + (subId ?? "__base__") + "|" + level;

    public static readonly Dictionary<string, Node> Map = new Dictionary<string, Node>(StringComparer.Ordinal)
    {
%s
    };

    public static bool TryGet(string skillId, string subId, int level, out Node node) =>
        Map.TryGetValue(Key(skillId, subId, level), out node);

    public static int UsedCost(IEnumerable<SkillBundle> known)
    {
        int total = 0;
        if (known == null) return total;
        foreach (SkillBundle bundle in known)
        {
            if (bundle.Levels == null) continue;
            foreach (KeyValuePair<string, int> pair in bundle.Levels)
            {
                for (int level = 1; level <= pair.Value; level++)
                {
                    if (TryGet(bundle.SkillId, pair.Key, level, out Node node)) total += node.SkillPoint;
                }
            }
        }
        return total;
    }
}
""" % "\n".join(rows)
out.write_text(text, encoding="utf-8")
print(f"wrote {out} ({count} nodes)")
