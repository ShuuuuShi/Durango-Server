using System;
using System.Collections.Generic;
using System.Text;
using JetBrains.Annotations;
using L10N;
using Messages;
using Shared.Ability;
using Shared.Skill;

namespace ItemSystem;

public class Util
{
	public enum SortOption
	{
		Default,
		Level,
		Weight,
		Durability,
		Color
	}

	public delegate void ItemDelegate(ItemData item);

	public delegate void ItemListDelegate(IList<ItemData> items);

	public const string UnknownIcon = "icon_question";

	public static string LocalizedTagRequiredMsg(IList<TagFilter> tagFilters, bool showLevel = true)
	{
		if (tagFilters == null || tagFilters.Count == 0)
		{
			return string.Empty;
		}
		List<string> list = new List<string>();
		for (int i = 0; i < tagFilters.Count; i++)
		{
			list.Add((!showLevel) ? TagData.GetTagName(tagFilters[i].TagId) : TagData.GetTagNameWithLevel(tagFilters[i]));
		}
		return T._("{0:l:{}|, | 또는 }", list);
	}

	public static string LocalizedDurability(float current, float max)
	{
		return string.Format(T._("{0:0.#}/{1:0.#}"), current, max);
	}

	public static string LocalizedModifiableCount(int modifiableCount)
	{
		return string.Format(T._("x{0}"), modifiableCount);
	}

	public static string GetItemModel(ItemData item, bool isMale)
	{
		string text;
		if (item == null)
		{
			text = null;
		}
		else
		{
			string key = ((!isMale) ? "female_model" : "male_model");
			text = item.GetStringAttribute(key);
			if (string.IsNullOrEmpty(text))
			{
				text = item.GetStringAttribute("model");
			}
			if (string.IsNullOrEmpty(text))
			{
				text = string.Empty;
			}
		}
		return text;
	}

	public static ulong[] ItemsToIds([NotNull] IList<ItemData> items)
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
		ulong[] array = new ulong[num];
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
		if (a.Like != b.Like)
		{
			return a.Like ? 1 : (-1);
		}
		int num = string.CompareOrdinal(a.Name, b.Name);
		return (num == 0) ? ItemBaseComparison(a, b) : num;
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
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		if (a.Colors.HasValue != b.Colors.HasValue)
		{
			return (!a.Colors.HasValue) ? 3 : (-3);
		}
		return UIUtility.ColorComparison(a.Colors[0], b.Colors[0]);
	}

	private static int ItemBaseComparison(ItemData a, ItemData b)
	{
		return (a.Id > b.Id) ? 1 : ((a.Id < b.Id) ? (-1) : 0);
	}

	public static List<ItemData> Filtering([NotNull] IList<ItemData> items, [NotNull] Func<ItemData, bool> func)
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

	public static int Counting([NotNull] IList<ItemData> items, [NotNull] Func<ItemData, bool> func)
	{
		int num = 0;
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			ItemData arg = items[i];
			if (func(arg))
			{
				num++;
			}
		}
		return num;
	}

	public static bool Exist([NotNull] IList<ItemData> items, [NotNull] Func<ItemData, bool> func)
	{
		int i = 0;
		for (int count = items.Count; i < count; i++)
		{
			ItemData arg = items[i];
			if (func(arg))
			{
				return true;
			}
		}
		return false;
	}

	public static int GetSlotCountBySuitableItem(ItemData itemData, [NotNull] IItemSlot[] slots)
	{
		int num = 0;
		for (int i = 0; i < slots.Length; i++)
		{
			if (slots[i].IsSuitableItem(itemData))
			{
				num++;
			}
		}
		return num;
	}

	public static int IndexOf(IList<ItemData> items, ulong id)
	{
		int i = 0;
		for (int num = items?.Count ?? 0; i < num; i++)
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
		if (info.ActionLevel <= 0)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(info.RelatedCategory);
		string text = "cbcbcb";
		if (info.PotentialLevel > categoryLevel)
		{
			text = "c40000";
		}
		else if (info.PotentialLevel < categoryLevel - 5)
		{
			text = "565656";
		}
		if (info.RelatedCategory != Category.Invalid)
		{
			string text2 = LocalizeUtil.Get(info.RelatedCategory);
			int categoryLevel2 = GameSystem<SkillSystem>.Instance().GetCategoryLevel(info.RelatedCategory);
			string format = ((!craft) ? "#item_collect_info_level" : "#item_craft_info_level");
			stringBuilder.AppendLine(LocalizeSystem.Format(format, info.ActionLevel.ToString(), text2, text, categoryLevel2.ToString()));
		}
		if (info.RelatedAbility != Derived.Invalid)
		{
			string text3 = LocalizeSystem.Get(LocalizeUtil.Get(info.RelatedAbility));
			GameSystem<StatisticsSystem>.Instance().DerivedAbilities.TryGetValue(info.RelatedAbility, out var value);
			stringBuilder.AppendLine(LocalizeSystem.Format("#action_info_ratio", info.SuccessRatio.ToString("P"), text3, value.ToString()));
		}
		return stringBuilder.ToString();
	}

	public static string ItemQualityString(ItemData item)
	{
		if (item == null)
		{
			return string.Empty;
		}
		StringBuilder stringBuilder = new StringBuilder();
		int i = 0;
		for (int count = item.TagModifications.Count; i < count; i++)
		{
			TagData tagData = item.TagModifications[i];
			int num = item.GetTagData(tagData.Id)?.Level ?? 0;
			int num2 = num + tagData.Level;
			string text = null;
			text = ((num2 == 0) ? T._("<em>{0} {1:lv:}</em>", tagData.LocalizedName, num.ToString()) : T._("<em>{0} {1:lv:} → {2:lv:}</em>", tagData.LocalizedName, num2.ToString(), num.ToString()));
			stringBuilder.AppendLine(text);
		}
		return stringBuilder.ToString().Trim();
	}
}
