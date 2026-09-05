using Shared.Skill;

namespace DurangoServer.Core;

/// <summary>
/// Skill nodes whose game data declares skill_point = 0 and untrain_disabled = true.
/// These are automatic progression rewards, not player choices.
///
/// Source: the client TextAsset named "skills". Keep rows in category/skill/level
/// order so a multi-level automatic branch can be granted in one pass.
/// </summary>
public static class AutomaticSkillData
{
    public readonly struct Node
    {
        public readonly Category Category;
        public readonly string SkillId;
        public readonly string SubId;
        public readonly int Level;
        public readonly int RequiredCategoryLevel;

        public Node(Category category, string skillId, string subId, int level, int requiredCategoryLevel)
        {
            Category = category;
            SkillId = skillId;
            SubId = subId;
            Level = level;
            RequiredCategoryLevel = requiredCategoryLevel;
        }
    }

    public static readonly Node[] Nodes =
    {
        new Node((Category)7, "leaf", "__base__", 1, 1),
        new Node((Category)7, "bough", "__base__", 1, 1),
        new Node((Category)7, "bush", "__base__", 1, 1),
        new Node((Category)7, "fruit", "__base__", 1, 1),
        new Node((Category)7, "forester", "__base__", 1, 1),
        new Node((Category)7, "bug", "__base__", 1, 1),
        new Node((Category)7, "stone", "__base__", 1, 1),
        new Node((Category)7, "stone", "__base__", 2, 5),
        new Node((Category)7, "clay", "__base__", 1, 1),
        new Node((Category)7, "stem", "__base__", 1, 1),
        new Node((Category)2, "kick", "__base__", 1, 1),
        new Node((Category)2, "reckless", "__base__", 1, 1),
        new Node((Category)12, "board", "__base__", 1, 1),
        new Node((Category)12, "shelter", "__base__", 1, 1),
        new Node((Category)12, "warphole_personal", "__base__", 1, 3),
        new Node((Category)12, "worktable", "__base__", 1, 1),
        new Node((Category)12, "furniture_box", "__base__", 1, 3),
        new Node((Category)12, "trap_epic", "__base__", 1, 1),
        new Node((Category)12, "sheaf", "__base__", 1, 3),
        new Node((Category)12, "bonfire", "__base__", 1, 1),
        new Node((Category)14, "basicwork", "__base__", 1, 1),
        new Node((Category)14, "rope", "__base__", 1, 1),
        new Node((Category)0, "adaptation", "__base__", 1, 5),
        new Node((Category)0, "adaptation", "__base__", 2, 10),
        new Node((Category)0, "adaptation", "__base__", 3, 20),
        new Node((Category)0, "adaptation", "__base__", 4, 30),
        new Node((Category)0, "adaptation", "__base__", 5, 40),
        new Node((Category)0, "adaptation", "__base__", 6, 50),
        new Node((Category)0, "adaptation", "__base__", 7, 60),
        new Node((Category)8, "cook_fire", "__base__", 1, 1),
        new Node((Category)4, "dodge", "__base__", 1, 1),
        new Node((Category)9, "sword_twohand", "__base__", 2, 15),
        new Node((Category)9, "axe_tool", "__base__", 1, 25),
        new Node((Category)9, "axe_twohand", "__base__", 2, 15),
        new Node((Category)9, "sword_onehand", "__base__", 1, 3),
        new Node((Category)9, "sword_onehand", "__base__", 2, 15),
        new Node((Category)9, "hammer_tool", "__base__", 1, 25),
        new Node((Category)9, "hammer_onehand", "__base__", 1, 3),
        new Node((Category)9, "axe_onehand", "__base__", 1, 2),
        new Node((Category)9, "axe_onehand", "__base__", 2, 15),
        new Node((Category)9, "blade_stone", "__base__", 1, 1),
        new Node((Category)9, "blade_stone", "__base__", 2, 25),
        new Node((Category)9, "club_woolen", "__base__", 1, 1),
        new Node((Category)15, "s02_constructing_shelter", "__base__", 1, 1),
        new Node((Category)15, "s02_constructing_shelter", "__base__", 2, 20),
        new Node((Category)15, "s02_tool", "__base__", 1, 1),
        new Node((Category)15, "s02_tool", "__base__", 2, 10),
        new Node((Category)15, "s02_tool", "__base__", 3, 12),
        new Node((Category)15, "s02_armorcrafting_clothes", "__base__", 1, 1),
        new Node((Category)15, "s02_armorcrafting_clothes", "__base__", 2, 3),
        new Node((Category)15, "s02_armorcrafting_clothes", "__base__", 3, 25),
        new Node((Category)15, "s02_armorcrafting_clothes", "__base__", 4, 30),
        new Node((Category)15, "s02_supplies", "__base__", 1, 1),
        new Node((Category)15, "s02_constructing", "__base__", 1, 1),
        new Node((Category)15, "s02_food", "__base__", 1, 1),
        new Node((Category)15, "s02_food", "__base__", 2, 5),
        new Node((Category)15, "s02_food", "__base__", 3, 10),
        new Node((Category)15, "s02_armorcrafting_accessory", "__base__", 1, 8),
        new Node((Category)15, "s02_armorcrafting_accessory", "__base__", 2, 20),
        new Node((Category)15, "s02_armorcrafting_accessory", "__base__", 3, 28),
        new Node((Category)15, "s02_material_process", "__base__", 1, 1),
        new Node((Category)13, "farming_corn", "__base__", 1, 1),
        new Node((Category)13, "farming_apple", "__base__", 1, 1),
        new Node((Category)5, "clam", "__base__", 1, 1),
        new Node((Category)5, "deboning", "__base__", 1, 1),
        new Node((Category)5, "cutting", "__base__", 1, 1),
        new Node((Category)5, "crab", "__base__", 1, 1),
        new Node((Category)5, "shrimp", "__base__", 1, 1),
        new Node((Category)10, "shoes", "__base__", 1, 1),
        new Node((Category)10, "clothes_novice", "__base__", 1, 1),
        new Node((Category)10, "sub_compi", "__base__", 1, 20),
    };
}
