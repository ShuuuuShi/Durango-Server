using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Skill;

namespace Yaml;

public class Reward
{
	[JsonProperty(PropertyName = "type")]
	public RewardType Type;

	[JsonProperty(PropertyName = "category")]
	public string Category;

	[JsonProperty(PropertyName = "category_level")]
	public int CategoryLevel;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "recipe_ids")]
	public string[] RecipeIds;

	[JsonProperty(PropertyName = "blueprint_ids")]
	public string[] BlueprintIds;

	[JsonProperty(PropertyName = "modifiers")]
	public Dictionary<string, float> Modifiers;

	[JsonProperty(PropertyName = "action_ids")]
	public string[] ActionIds;

	[JsonProperty(PropertyName = "modifier")]
	public string Modifier;

	[JsonProperty(PropertyName = "value")]
	public float Value;

	[JsonProperty(PropertyName = "seed_id")]
	public string SeedId;

	[JsonProperty(PropertyName = "entity_types")]
	public int[] EntityTypes;
}
