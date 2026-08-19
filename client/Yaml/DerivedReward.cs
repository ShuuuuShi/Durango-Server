using Building;
using Crafting;
using L10N;
using Newtonsoft.Json;
using Shared.Faction;
using Yaml.Util;

namespace Yaml;

public class DerivedReward
{
	[JsonProperty(PropertyName = "type")]
	public RewardType Type;

	[JsonProperty(PropertyName = "modifier_id")]
	public string ModifierId;

	[JsonProperty(PropertyName = "value")]
	public float Value;

	[JsonProperty(PropertyName = "recipe_id")]
	public string RecipeId;

	[JsonProperty(PropertyName = "blueprint_id")]
	public string BlueprintId;

	public string ToDescription()
	{
		switch (Type)
		{
		case RewardType.Recipe:
		case RewardType.DynamicRecipe:
		{
			Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(RecipeId);
			return (recipe == null || !recipe.Available) ? T._("{0} 제작법 획득", (recipe != null) ? recipe.Name : RecipeId) : string.Format("<ref>ui://Recipe/Crafting/{1},{0}</ref>", recipe.Name, RecipeId);
		}
		case RewardType.Blueprint:
		case RewardType.DynamicBlueprint:
		{
			Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(BlueprintId);
			string text = ((blueprint != null) ? blueprint.Name : BlueprintId);
			return (blueprint == null || !blueprint.Available) ? T._("{0} 건설법 획득", (blueprint != null) ? blueprint.Name : BlueprintId) : string.Format("<ref>ui://Recipe/Building/{1},{0}</ref>", blueprint.Name, BlueprintId);
		}
		case RewardType.DynamicModifier:
		{
			SkillModifier skillModifier = SingletonDict<string, SkillModifier>.Get(ModifierId);
			if (skillModifier == null)
			{
				return $"{ModifierId} {Value:0.#}";
			}
			return string.Format("{0} {1}", skillModifier.Name, skillModifier.GetValueString(Value, null, "[icon=img_pet_arrow_up] {0}", "[icon=img_pet_arrow_down] {0}"));
		}
		default:
			return string.Empty;
		}
	}
}
