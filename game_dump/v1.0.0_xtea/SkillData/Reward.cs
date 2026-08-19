using System.Collections.Generic;
using System.Text;
using Building_;
using Crafting;
using L10N;
using Shared.Skill;
using Yaml;
using Yaml.Util;

namespace SkillData;

public class Reward
{
	public readonly string Id;

	public readonly RewardType Type;

	public string Category;

	public int CategoryLevel;

	public string Name;

	public string[] RecipeIds;

	public string[] BlueprintIds;

	public Dictionary<string, float> Modifiers;

	public string[] Tags;

	public string Modifier;

	public float Value;

	public string SeedId;

	public Dictionary<string, float> ActionPolicies;

	public string[] ActionSets;

	public Reward(string key, Yaml.Reward data)
	{
		Id = key;
		Type = data.type;
		Category = data.category;
		CategoryLevel = data.category_level;
		Name = data.name;
		RecipeIds = data.recipe_ids;
		BlueprintIds = data.blueprint_ids;
		Modifiers = data.modifiers;
		Tags = data.tags;
		Modifier = data.modifier;
		Value = data.value;
		SeedId = data.seed_id;
		ActionPolicies = data.action_policies;
		ActionSets = data.action_sets;
	}

	public string ToReadableText()
	{
		StringBuilder stringBuilder = new StringBuilder();
		switch (Type)
		{
		case RewardType.Generator:
			stringBuilder.AppendLine(T._("[f1eee8]{0}[-] [bcb4a8]채집 요령[-]", Name));
			break;
		case RewardType.Recipe:
		{
			StringBuilder stringBuilder3 = new StringBuilder();
			int j = 0;
			for (int num2 = ((RecipeIds != null) ? RecipeIds.Length : 0); j < num2; j++)
			{
				string id = RecipeIds[j];
				Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(id);
				if (recipe != null)
				{
					if (stringBuilder3.Length > 0)
					{
						stringBuilder3.Append(", ");
					}
					stringBuilder3.Append(recipe.LocalizedName);
				}
			}
			stringBuilder.Append(T._("[f1eee8]{0}[-] [bcb4a8]제작 방법[-]", stringBuilder3));
			break;
		}
		case RewardType.Blueprint:
		{
			StringBuilder stringBuilder5 = new StringBuilder();
			int l = 0;
			for (int num4 = ((BlueprintIds != null) ? BlueprintIds.Length : 0); l < num4; l++)
			{
				string id2 = BlueprintIds[l];
				Building_.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(id2);
				if (blueprint != null)
				{
					if (stringBuilder5.Length > 0)
					{
						stringBuilder5.Append(", ");
					}
					stringBuilder5.Append(blueprint.LocalizedName);
				}
			}
			stringBuilder.AppendLine(T._("[f1eee8]{0}[-] [bcb4a8]설치 방법[-]", stringBuilder5));
			break;
		}
		case RewardType.Action:
		{
			StringBuilder stringBuilder6 = new StringBuilder();
			int m = 0;
			for (int num5 = ((Tags != null) ? Tags.Length : 0); m < num5; m++)
			{
				string tag2 = Tags[m];
				if (stringBuilder6.Length != 0)
				{
					stringBuilder6.Append(" ");
				}
				stringBuilder6.Append(LocalizeUtil.ActionTagName(tag2));
			}
			stringBuilder.AppendLine(T._("[f1eee8]{0}[-] [fad257]습득[-]", stringBuilder6));
			break;
		}
		case RewardType.ActionPolicy:
			if (ActionPolicies == null)
			{
				break;
			}
			foreach (KeyValuePair<string, float> actionPolicy2 in ActionPolicies)
			{
				int num = (int)actionPolicy2.Value;
				ActionPolicy actionPolicy = SingletonDict<string, ActionPolicy>.Get(actionPolicy2.Key);
				string text = ((actionPolicy != null) ? actionPolicy.name.ToString() : actionPolicy2.Key);
				stringBuilder.AppendLine((num != 1) ? T._("[f1eee8]{0} 전술[-] [fad257]레벨 {1}[-] [bcb4a8]로 상승[-]", text, num) : T._("[f1eee8]{0} 전술[-] [fad257]습득[-]", text));
			}
			break;
		case RewardType.ActionEnhancement:
		{
			StringBuilder stringBuilder4 = new StringBuilder();
			int k = 0;
			for (int num3 = ((Tags != null) ? Tags.Length : 0); k < num3; k++)
			{
				string tag = Tags[k];
				if (stringBuilder4.Length != 0)
				{
					stringBuilder4.Append(" ");
				}
				stringBuilder4.Append(LocalizeUtil.ActionTagName(tag));
			}
			string text2 = Value.ToString((!(Value > 1f)) ? "0%" : "0");
			switch (SingletonDict<string, SkillModifier>.Get(Modifier).reduce_type)
			{
			case "sum":
			{
				string text3 = ((!SingletonDict<string, SkillModifier>.Get(Modifier).inverse) ? "+" : "-");
				stringBuilder.AppendLine(T._("[f1eee8]{0}[-] [bcb4a8]{1}[-] [fad257]{2}{3}[-]", stringBuilder4, LocalizeUtil.ModifierName(Modifier), text3, text2));
				break;
			}
			default:
				stringBuilder.AppendLine(T._("[f1eee8]{0}[-] [bcb4a8]{1}[-] [fad257]{2}[-]", stringBuilder4, LocalizeUtil.ModifierName(Modifier), text2));
				break;
			}
			break;
		}
		case RewardType.CombatCapability:
		case RewardType.SocialCapability:
		case RewardType.LivingCapability:
			foreach (KeyValuePair<string, float> modifier in Modifiers)
			{
				switch (SingletonDict<string, SkillModifier>.Get(modifier.Key).reduce_type)
				{
				case "sum":
					stringBuilder.AppendLine(T._("[f1eee8]{0}[-] [fad257]+{1}[-]", LocalizeUtil.ModifierName(modifier.Key), modifier.Value.ToString("0.#")));
					break;
				default:
					stringBuilder.AppendLine(T._("[f1eee8]{0}[-] [fad257]{1}[-]", LocalizeUtil.ModifierName(modifier.Key), modifier.Value.ToString("0.#")));
					break;
				}
			}
			break;
		case RewardType.Farming:
		{
			Prototype itemPrototype = PrototypeYaml.GetItemPrototype(SeedId, 1);
			if (itemPrototype != null)
			{
				stringBuilder.Append(T._("[f1eee8]{0}[-] [bcb4a8]재배 가능[-]", itemPrototype.name));
			}
			break;
		}
		case RewardType.AdditionalActionSet:
		{
			StringBuilder stringBuilder2 = new StringBuilder();
			for (int i = 0; i < ActionSets.Length; i++)
			{
				string key = ActionSets[i];
				if (SingletonDict<string, ActionSet>.TryGetValue(key, out var value))
				{
					if (stringBuilder2.Length != 0)
					{
						stringBuilder2.Append(" ");
					}
					stringBuilder2.Append(value.name);
				}
			}
			stringBuilder.AppendLine(T._("[f1eee8]{0}[-] [fad257]습득[-]", stringBuilder2));
			break;
		}
		}
		if (stringBuilder.Length == 0)
		{
			stringBuilder.AppendFormat("{0}: {1}", Type, Id);
		}
		return stringBuilder.ToString().Trim();
	}
}
