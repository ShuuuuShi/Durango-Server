using System;
using System.Collections.Generic;
using System.Text;
using Building;
using Durango.Model;
using Durango.UI.Control;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Building;
using Shared.Etc;
using Shared.Skill;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.Logic.Item;

public static class Util
{
	public delegate void ItemDelegate(ItemData item);

	public delegate void ItemListDelegate(IList<ItemData> items);

	public enum SortOption
	{
		[T.EnumName("자동 정렬")]
		Default,
		[T.EnumName("레벨 정렬")]
		Level,
		[T.EnumName("크기 정렬")]
		Weight,
		[T.EnumName("내구도 정렬")]
		Durability,
		[T.EnumName("색깔 정렬")]
		Color
	}

	public const string UnknownIcon = "icon_question";

	private static readonly Dictionary<string, int> PriorCategoryOrder = new Dictionary<string, int>
	{
		{ "weapon/tool", 1 },
		{ "clothing", 2 },
		{ "accessory", 3 }
	};

	public static string LocalizedTagRequiredMsg(OrTagFilter tagFilters, bool showLevel = true)
	{
		if (tagFilters == null || tagFilters.Tags.Count == 0)
		{
			return string.Empty;
		}
		using Reusable<List<string>> reusable = ReusableList<string>.Pop();
		List<string> value = reusable.Value;
		for (int i = 0; i < tagFilters.Tags.Count; i++)
		{
			TagFilterBase.Tag tag = tagFilters.Tags[i];
			value.Add((!showLevel || tag.Level <= 1) ? TagData.GetTagName(tag.Id) : TagData.GetTagNameWithLevel(tag.Id, tag.Level));
		}
		return T._("{0:l:{}|, }", value);
	}

	public static string LocalizedTagNamesAndLevels(IEnumerable<KeyValuePair<string, int>> tagIds)
	{
		if (tagIds == null)
		{
			return string.Empty;
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		foreach (KeyValuePair<string, int> tagId in tagIds)
		{
			value.Append($"<tag>{tagId.Key},{tagId.Value}</tag>[size=40] [/size]");
		}
		return value.ToString();
	}

	public static string LocalizedDurability(float current, float max)
	{
		return string.Format((!(current > 0f)) ? "[c][A13A2B]{0}[-][/c]/{1}" : "{0}/{1}", T._("{0:0.#}", current), T._("{0:0.#}", max));
	}

	public static string LocalizedModifiableCount(int modifiableCount)
	{
		return string.Format(T._("x{0}"), modifiableCount);
	}

	public static bool IsCapturable(this ItemData item)
	{
		if (item == null)
		{
			return false;
		}
		return !item.IsDestroyed() && item.HasTag("capturable");
	}

	public static IEnumerable<Pair<string, int>> GetStatusEffects(this ItemData item)
	{
		foreach (Performance p in item.Performances)
		{
			if (p.Strs.TryGetValue("effect_on", out var effectOn) && p.Nums.TryGetValue("effect_on_level", out var effectOnLevel))
			{
				yield return new Pair<string, int>(effectOn, (int)effectOnLevel);
			}
		}
	}

	public static string GetModel(this ItemData item, bool isMale)
	{
		string text;
		if (item == null)
		{
			text = null;
		}
		else
		{
			string stringAttribute = item.GetStringAttribute("weapon_framework");
			if (!string.IsNullOrEmpty(stringAttribute) && stringAttribute.Equals("barehand", StringComparison.OrdinalIgnoreCase))
			{
				text = string.Empty;
			}
			else
			{
				string key = ((!isMale) ? "female_model" : "male_model");
				text = item.GetStringAttribute(key);
				if (string.IsNullOrEmpty(text))
				{
					text = item.GetStringAttribute("model");
				}
				if (string.IsNullOrEmpty(text) || text.Equals("None", StringComparison.OrdinalIgnoreCase))
				{
					text = null;
				}
			}
		}
		return text;
	}

	public static bool HasPreview(this ItemData item)
	{
		return item.SetPreview(null);
	}

	public static bool SetPreview(this ItemData item, [CanBeNull] UIModelViewer viewer, Action<GameObject> loaded = null)
	{
		if (item == null)
		{
			return false;
		}
		int petEntityType = item.GetPetEntityType();
		if (petEntityType != 0)
		{
			if (viewer != null)
			{
				Yaml.Pet pet = SingletonDict<int, Yaml.Pet>.Get(petEntityType);
				Action<GameObject> a = null;
				if (pet != null && pet.IsRidable)
				{
					a = (Action<GameObject>)Delegate.Combine(a, viewer.SetupSaddle());
				}
				a = (Action<GameObject>)Delegate.Combine(a, viewer.DefaultAnimalPlay("idle", "stand"));
				if (loaded != null)
				{
					a = (Action<GameObject>)Delegate.Combine(a, loaded);
				}
				viewer.SetPlainModel((pet != null) ? AnimalYaml.GetPrefabPath(pet.VehicleEntityType) : null, new UIModelViewer.Arguments
				{
					CameraAngle = 35f,
					Rotation = 140f,
					Loaded = a
				});
			}
			return true;
		}
		if (item.HasTag("dye_medicine"))
		{
			if (viewer != null)
			{
				ArtifactDisplay artifactDisplay = default(ArtifactDisplay);
				artifactDisplay.Condition = Shared.Building.Condition.Normal;
				artifactDisplay.Parts = new Dictionary<string, string> { { "common", "dye_02" } };
				artifactDisplay.Decorations = new Dictionary<string, Pair<string, string>> { 
				{
					"__workbench__",
					new Pair<string, string>("dye_02_color", item.Colors.GetColor(0, origin: true).ToHex())
				} };
				ArtifactDisplay display = artifactDisplay;
				viewer.SetArtifactModel(new UIModelViewer.ArtifactArguments
				{
					Display = display,
					IsModular = false,
					Size = new Point2(1, 1),
					Rotation = Rotation.None
				}, new UIModelViewer.Arguments
				{
					CameraAngle = 35f,
					Rotation = -45f,
					Loaded = loaded
				});
			}
			return true;
		}
		if (item.Capsule.HasValue)
		{
			if (viewer != null)
			{
				ArtifactCapsule value = item.Capsule.Value;
				Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(value.BlueprintId);
				if (blueprint != null)
				{
					ArtifactDisplay display2 = value.Display;
					if (display2.Parts == null)
					{
						display2 = blueprint.GetDefaultDisplay();
					}
					Point2 size = ((!blueprint.IsSizeVariable) ? blueprint.Size : Point2.one);
					viewer.SetArtifactModel(new UIModelViewer.ArtifactArguments
					{
						Display = display2,
						IsModular = blueprint.IsModular,
						Size = size,
						Rotation = Rotation.None
					}, new UIModelViewer.Arguments
					{
						CameraAngle = 35f,
						Rotation = -45f,
						Loaded = loaded
					});
				}
			}
			return true;
		}
		string model = item.GetModel(PlayerBehavior.LocalPlayer.IsMale);
		if (!string.IsNullOrEmpty(model))
		{
			if (viewer != null)
			{
				PlayerDisplay display3 = PlayerBehavior.LocalPlayer.Display;
				string stringAttribute = item.GetStringAttribute("weapon_framework");
				PlayerBehavior.WeaponFramework framework;
				if (string.IsNullOrEmpty(stringAttribute))
				{
					framework = PlayerBehavior.WeaponFramework.BAREHAND;
					switch (CharacterCostume.GetCostumeType(model))
					{
					case CharacterCostume.CostumeType.Head:
						display3.Head = model;
						display3.HeadColor = item.Colors.ToHexes();
						break;
					case CharacterCostume.CostumeType.Body:
						display3.Body = model;
						display3.BodyColor = item.Colors.ToHexes();
						break;
					case CharacterCostume.CostumeType.Beard:
						display3.Beard = model;
						break;
					case CharacterCostume.CostumeType.Hair:
						display3.Hair = model;
						break;
					}
					display3.Equip = null;
				}
				else
				{
					framework = stringAttribute.ToEnum(PlayerBehavior.WeaponFramework.BAREHAND);
					display3.Equip = model;
					display3.EquipColor = item.Colors.ToHexes();
				}
				Action<GameObject> action = delegate(GameObject obj)
				{
					PlayerBehavior component = obj.GetComponent<PlayerBehavior>();
					if (!(component == null) && !(component.Anim == null))
					{
						component.ChangeWeaponType(framework);
					}
				};
				if (loaded != null)
				{
					action = (Action<GameObject>)Delegate.Combine(action, loaded);
				}
				viewer.SetPlayerModel(PlayerBehavior.LocalPlayer.IsMale, display3, new UIModelViewer.Arguments
				{
					CameraAngle = 35f,
					Rotation = 140f,
					Loaded = action
				});
			}
			return true;
		}
		return false;
	}

	public static int GetPetEntityType(this ItemData item)
	{
		int result = 0;
		if (item != null)
		{
			result = (int)item.GetFloatAttribute("pet_entity_type");
		}
		return result;
	}

	public static string[] ItemsToIds([NotNull] IList<ItemData> items)
	{
		int num = 0;
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			if (items[i] != null)
			{
				num++;
			}
		}
		string[] array = new string[num];
		int num2 = 0;
		int j = 0;
		for (int count2 = items.Count; j < count2; j++)
		{
			if (items[j] != null)
			{
				array[num2++] = items[j].Id;
			}
		}
		return array;
	}

	public static void SortItems(List<ItemData> itemList, SortOption option = SortOption.Default, bool descending = false)
	{
		if (itemList == null)
		{
			return;
		}
		Comparison<ItemData> itemComparison = GetItemComparison(option);
		if (itemComparison != null)
		{
			itemList.Sort(itemComparison);
			if (descending)
			{
				itemList.Reverse();
			}
		}
	}

	public static Comparison<ItemData> GetItemComparison(SortOption option)
	{
		Comparison<ItemData> result = null;
		switch (option)
		{
		case SortOption.Default:
			result = ItemDefaultComparison;
			break;
		case SortOption.Level:
			result = ItemLevelComparison;
			break;
		case SortOption.Durability:
			result = ItemDurabilityComparison;
			break;
		case SortOption.Weight:
			result = ItemWeightComparison;
			break;
		case SortOption.Color:
			result = ItemColorComprison;
			break;
		}
		return result;
	}

	private static int ItemDefaultComparison(ItemData a, ItemData b)
	{
		if (a.IsEquipments != b.IsEquipments)
		{
			return a.IsEquipments ? 1 : (-1);
		}
		if (a.SafeLevel != b.SafeLevel)
		{
			return (a.SafeLevel > b.SafeLevel) ? 1 : (-1);
		}
		if (a.IsDestroyed() != b.IsDestroyed())
		{
			return a.IsDestroyed() ? 1 : (-1);
		}
		if (a.Unstable != b.Unstable)
		{
			return (!a.Unstable) ? 1 : (-1);
		}
		int num = ComparePrototype(a, b);
		if (num != 0)
		{
			return num;
		}
		if (a.ModifiedCount - b.ModifiedCount != 0)
		{
			return b.ModifiedCount - a.ModifiedCount;
		}
		if (a.Level - b.Level != 0)
		{
			return a.Level - b.Level;
		}
		float num2 = a.Durability.Get();
		float num3 = b.Durability.Get();
		if (Math.Abs(num2 - num3) > float.Epsilon)
		{
			return Math.Sign(num3 - num2);
		}
		int num4 = string.CompareOrdinal(a.Name, b.Name);
		return (num4 == 0) ? ItemBaseComparison(a, b) : num4;
	}

	private static int ComparePrototype(ItemData a, ItemData b)
	{
		if (a.Prototype == b.Prototype)
		{
			return 0;
		}
		if (a.Prototype == null || b.Prototype == null)
		{
			return (a.Prototype != null) ? 1 : (-1);
		}
		string category = a.Prototype.Category;
		string category2 = b.Prototype.Category;
		if (category != category2)
		{
			int num = PriorCategoryOrder.Get(category, int.MaxValue);
			int num2 = PriorCategoryOrder.Get(category2, int.MaxValue);
			if (num != num2)
			{
				return num2 - num;
			}
			int num3 = string.CompareOrdinal(a.Prototype.Category, b.Prototype.Category);
			if (num3 != 0)
			{
				return num3;
			}
		}
		string[] subCategories = a.Prototype.SubCategories;
		string[] subCategories2 = b.Prototype.SubCategories;
		int size = KUtility.GetSize(subCategories);
		int size2 = KUtility.GetSize(subCategories2);
		if (size == size2)
		{
			for (int i = 0; i < size; i++)
			{
				int num4 = string.CompareOrdinal(subCategories[i], subCategories2[i]);
				if (num4 != 0)
				{
					return num4;
				}
			}
			return string.CompareOrdinal(a.PrototypeName, b.PrototypeName);
		}
		return size - size2;
	}

	private static int ItemLevelComparison(ItemData a, ItemData b)
	{
		int num = a.Level - b.Level;
		return (num == 0) ? ItemBaseComparison(a, b) : num;
	}

	private static int ItemDurabilityComparison(ItemData a, ItemData b)
	{
		int num = (int)(a.Durability.Get() * 100f) - (int)(b.Durability.Get() * 100f);
		return (num == 0) ? ItemBaseComparison(a, b) : num;
	}

	private static int ItemWeightComparison(ItemData a, ItemData b)
	{
		int num = (int)((float)a.Size * 100f) - (int)((float)b.Size * 100f);
		return (num == 0) ? ItemBaseComparison(a, b) : num;
	}

	private static int ItemColorComprison(ItemData a, ItemData b)
	{
		if (a.Colors.HasValue != b.Colors.HasValue)
		{
			return (!a.Colors.HasValue) ? 3 : (-3);
		}
		return UIUtility.ColorComparison(a.Colors[0], b.Colors[0]);
	}

	private static int ItemBaseComparison(ItemData a, ItemData b)
	{
		return string.CompareOrdinal(a.Id, b.Id);
	}

	public static List<ItemData> Filtering([NotNull] IList<ItemData> items, [NotNull] Predicate<ItemData> func)
	{
		List<ItemData> list = new List<ItemData>();
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			ItemData itemData = items[i];
			if (func(itemData))
			{
				list.Add(itemData);
			}
		}
		return list;
	}

	public static int Counting([NotNull] IList<ItemData> items, [NotNull] Predicate<ItemData> func)
	{
		int num = 0;
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			ItemData obj = items[i];
			if (func(obj))
			{
				num++;
			}
		}
		return num;
	}

	public static bool Exist([NotNull] IList<ItemData> items, [NotNull] Predicate<ItemData> func)
	{
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			ItemData obj = items[i];
			if (func(obj))
			{
				return true;
			}
		}
		return false;
	}

	public static int IndexOf(IList<ItemData> items, string id)
	{
		int i = 0;
		for (int size = KUtility.GetSize(items); i < size; i++)
		{
			if (items[i] != null && items[i].Id == id)
			{
				return i;
			}
		}
		return -1;
	}

	public static string ActionInfoDetailString(ActionInfo info, bool craft = false)
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(info.SuccessRatio.ToString("P0"));
		if (info.RelatedCategory != Category.Invalid)
		{
			string arg = LocalizeUtil.Get(info.RelatedCategory);
			int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(info.RelatedCategory);
			stringBuilder.Append($" {arg}: {categoryLevel}");
		}
		if (info.RelatedAbility != Derived.Invalid)
		{
			string arg2 = LocalizeUtil.Get(info.RelatedAbility);
			float deriveds = GameSystem<StatisticsSystem>.Instance().GetDeriveds(info.RelatedAbility);
			stringBuilder.Append($" {arg2}: {(int)deriveds}");
		}
		return stringBuilder.ToString();
	}

	public static string ItemQualityString(Messages.Item item)
	{
		StringBuilder stringBuilder = new StringBuilder();
		int num = 0;
		int i = 0;
		for (int size = KUtility.GetSize(item.TagModifications); i < size; i++)
		{
			Messages.Tag tag = item.TagModifications[i];
			int num2 = -1;
			int j = 0;
			for (int size2 = KUtility.GetSize(item.Tags); j < size2; j++)
			{
				if (item.Tags[j].Id == tag.Id)
				{
					num2 = j;
					break;
				}
			}
			int num3 = ((num2 != -1) ? item.Tags[num2].Level : 0);
			int num4 = num3 - tag.Level;
			Yaml.Tag tag2 = SingletonDict<string, Yaml.Tag>.Get(tag.Id);
			if (tag2 != null && tag2.Visible)
			{
				string text = tag2.Name.ToString();
				string value = ((!tag2.VisibleLevel) ? text : ((num4 == 0) ? T._("{0} {1:lv:}", text, num3) : T._("{0} {1:lv:} → {2:lv:}", text, num4, num3)));
				if (num > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(value);
				num++;
			}
		}
		return stringBuilder.ToString().Trim();
	}

	public static HashSet<string> SelectManyTags(IList<ItemData> items)
	{
		HashSet<string> hashSet = new HashSet<string>();
		int i = 0;
		for (int size = KUtility.GetSize(items); i < size; i++)
		{
			List<TagData> tags = items[i].Tags;
			foreach (TagData item in tags)
			{
				hashSet.Add(item.Id);
			}
		}
		return hashSet;
	}
}
