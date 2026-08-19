using System.Collections.Generic;
using System.Text;
using Building;
using Crafting;
using Durango.Utils;
using L10N;
using Shared.Skill;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Skill;

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

	public string Modifier;

	public float Value;

	public string SeedId;

	public string[] ActionIds;

	public int[] EntityTypes;

	public Reward(string key, Yaml.Reward data)
	{
		Id = key;
		Type = data.Type;
		Category = data.Category;
		CategoryLevel = data.CategoryLevel;
		Name = data.Name;
		RecipeIds = data.RecipeIds;
		BlueprintIds = data.BlueprintIds;
		Modifiers = data.Modifiers;
		Modifier = data.Modifier;
		Value = data.Value;
		SeedId = data.SeedId;
		ActionIds = data.ActionIds;
		EntityTypes = data.EntityTypes;
	}

	public void ToReadableText(StringBuilder result, State skillState)
	{
		ToReadableText(result, skillState, hasLink: false);
	}

	public void ToReadableLinkText(StringBuilder result, State skillState)
	{
		ToReadableText(result, skillState, hasLink: true);
	}

	private void ToReadableText(StringBuilder result, State skillState, bool hasLink)
	{
		switch (Type)
		{
		case RewardType.Generator:
			result.AppendLine(T._("[f1eee8]{0}[-] [bcb4a8]채집 요령[-]", Name));
			break;
		case RewardType.Recipe:
		{
			using (Reusable<List<string>> reusable3 = ReusableList<string>.Pop())
			{
				List<string> value3 = reusable3.Value;
				int k = 0;
				for (int size2 = KUtility.GetSize(RecipeIds); k < size2; k++)
				{
					string text2 = RecipeIds[k];
					Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(text2);
					if (recipe != null && (skillState != State.Learned || recipe.Available))
					{
						if (hasLink)
						{
							value3.Add(string.Format("<ref>ui://Recipe/Crafting/{1},{0}</ref>", recipe.Name, text2));
						}
						else
						{
							value3.Add(recipe.Name);
						}
					}
				}
				if (value3.Count > 0)
				{
					result.AppendLine(T._("[f1eee8]{0:l:{}|, }[-] [bcb4a8]제작 방법[-]", value3));
				}
			}
			break;
		}
		case RewardType.Blueprint:
		{
			using (Reusable<List<string>> reusable2 = ReusableList<string>.Pop())
			{
				List<string> value2 = reusable2.Value;
				int j = 0;
				for (int size = KUtility.GetSize(BlueprintIds); j < size; j++)
				{
					string text = BlueprintIds[j];
					Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(text);
					if (blueprint != null && (skillState != State.Learned || blueprint.Available))
					{
						if (hasLink)
						{
							value2.Add(string.Format("<ref>ui://Recipe/Building/{1},{0}</ref>", blueprint.Name, text));
						}
						else
						{
							value2.Add(blueprint.Name);
						}
					}
				}
				if (value2.Count > 0)
				{
					result.AppendLine(T._("[f1eee8]{0:l:{}|, }[-] [bcb4a8]설치 방법[-]", value2));
				}
			}
			break;
		}
		case RewardType.Action:
			result.AppendLine(T._("[f1eee8]{0}[-] [fad257]습득[-]", Name));
			break;
		case RewardType.ActionEnhancement:
		{
			SkillModifier skillModifier2 = SingletonDict<string, SkillModifier>.Get(Modifier);
			if (skillModifier2 != null)
			{
				string reduceType2 = skillModifier2.ReduceType;
				if (reduceType2 != null && reduceType2 == "sum")
				{
					result.AppendLine(T._("[bcb4a8]{0}[-] [fad257]{1}[-]", skillModifier2.Name, skillModifier2.GetValueString(Value)));
				}
				else
				{
					result.AppendLine(T._("[bcb4a8]{0}[-] [fad257]{1}[-]", skillModifier2.Name, Value.ToString(skillModifier2.GetValueFormat())));
				}
			}
			break;
		}
		case RewardType.CombatCapability:
		case RewardType.SocialCapability:
		case RewardType.LivingCapability:
		case RewardType.WeaponEnhancement:
			foreach (KeyValuePair<string, float> modifier in Modifiers)
			{
				SkillModifier skillModifier = SingletonDict<string, SkillModifier>.Get(modifier.Key);
				if (skillModifier != null)
				{
					string reduceType = skillModifier.ReduceType;
					if (reduceType != null && reduceType == "sum")
					{
						result.AppendLine(T._("[f1eee8]{0}[-] [fad257]+{1}[-]", skillModifier.Name, modifier.Value.ToString("0.#")));
					}
					else
					{
						result.AppendLine(T._("[f1eee8]{0}[-] [fad257]{1}[-]", skillModifier.Name, modifier.Value.ToString("0.#")));
					}
				}
			}
			break;
		case RewardType.Farming:
		{
			Prototype itemPrototype = PrototypeYaml.GetItemPrototype(SeedId, 1);
			if (itemPrototype != null)
			{
				result.AppendLine(T._("[f1eee8]{0}[-] [bcb4a8]재배 가능[-]", itemPrototype.Name));
			}
			break;
		}
		case RewardType.PetTameable:
		{
			using (Reusable<List<string>> reusable = ReusableList<string>.Pop())
			{
				List<string> value = reusable.Value;
				for (int i = 0; i < EntityTypes.Length; i++)
				{
					value.Add(AnimalYaml.GetName(EntityTypes[i]));
				}
				result.AppendLine(T._("[f1eee8]{0:l:{}|, }[-] [fad257]포획 기술 습득[-]", value));
			}
			break;
		}
		}
		if (result.Length == 0)
		{
			result.AppendFormat("{0}: {1}\n", Type, Id);
		}
	}
}
