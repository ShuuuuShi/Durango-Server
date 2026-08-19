using System.Collections.Generic;
using Shared.Skill;

namespace Yaml;

public class Reward
{
	public RewardType type;

	public string category;

	public int category_level;

	public Gettext name;

	public string[] recipe_ids;

	public string[] blueprint_ids;

	public Dictionary<string, float> modifiers;

	public string[] tags;

	public string modifier;

	public float value;

	public string seed_id;

	public Dictionary<string, float> action_policies;

	public string[] action_sets;
}
