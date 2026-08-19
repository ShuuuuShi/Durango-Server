using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Yaml.Util;

namespace Yaml;

public class ShopCategories : Singleton<ShopCategories>
{
	[JsonProperty(PropertyName = "shop_ui_categories")]
	public ShopCategory[] Categories;

	[JsonProperty(PropertyName = "shop_ui_options")]
	public ShopUIOption ShopUIOptions;

	public static ShopCategory[] GetCategories()
	{
		if (Singleton<ShopCategories>.Instance == null)
		{
			return null;
		}
		return Singleton<ShopCategories>.Instance.Categories;
	}

	public static ShopCategory FindCategory([NotNull] Predicate<ShopCategory> predicate)
	{
		return FindCategory(Singleton<ShopCategories>.Instance.Categories, predicate);
	}

	public static ShopCategory FindCategory([NotNull] IEnumerable<ShopCategory> categories, [NotNull] Predicate<ShopCategory> predicate)
	{
		foreach (ShopCategory category in categories)
		{
			if (KUtility.GetSize(category.Childs) == 0)
			{
				if (predicate(category))
				{
					return category;
				}
				continue;
			}
			ShopCategory shopCategory = FindCategory(category.Childs, predicate);
			if (shopCategory == null)
			{
				continue;
			}
			return shopCategory;
		}
		return null;
	}

	public static ShopCategory FindCategory(string key)
	{
		ShopCategory[] categories = GetCategories();
		foreach (ShopCategory shopCategory in categories)
		{
			if (shopCategory.Key == key)
			{
				return shopCategory;
			}
			ShopCategory shopCategory2 = shopCategory.FindChild(key);
			if (shopCategory2 != null)
			{
				return shopCategory2;
			}
		}
		return null;
	}

	public static bool IsShowTradeLock(ItemData item)
	{
		if (item.Tradable)
		{
			return false;
		}
		if (Singleton<ShopCategories>.Instance == null || Singleton<ShopCategories>.Instance.ShopUIOptions == null || Singleton<ShopCategories>.Instance.ShopUIOptions.ShowTradable == null)
		{
			return true;
		}
		return Singleton<ShopCategories>.Instance.ShopUIOptions.ShowTradable.IsValid(item);
	}

	public static bool IsShowDumpLock(ItemData item)
	{
		if (item.Dumpable)
		{
			return false;
		}
		if (Singleton<ShopCategories>.Instance == null || Singleton<ShopCategories>.Instance.ShopUIOptions == null || Singleton<ShopCategories>.Instance.ShopUIOptions.ShowDumpable == null)
		{
			return true;
		}
		return Singleton<ShopCategories>.Instance.ShopUIOptions.ShowDumpable.IsValid(item);
	}

	public static bool IsShowDyeLock(ItemData item)
	{
		if (item.IsDyeable())
		{
			return false;
		}
		if (Singleton<ShopCategories>.Instance == null || Singleton<ShopCategories>.Instance.ShopUIOptions == null || Singleton<ShopCategories>.Instance.ShopUIOptions.ShowDyeable == null)
		{
			return true;
		}
		return Singleton<ShopCategories>.Instance.ShopUIOptions.ShowDyeable.IsValid(item);
	}

	public static bool IsShowRepairLock(ItemData item)
	{
		if (item.IsRepairable)
		{
			return false;
		}
		if (Singleton<ShopCategories>.Instance == null || Singleton<ShopCategories>.Instance.ShopUIOptions == null || Singleton<ShopCategories>.Instance.ShopUIOptions.ShowRepairable == null)
		{
			return true;
		}
		return Singleton<ShopCategories>.Instance.ShopUIOptions.ShowRepairable.IsValid(item);
	}

	public static bool IsShowAvator(ItemData item)
	{
		if (!item.HasTag("equipment_avatar"))
		{
			return false;
		}
		if (Singleton<ShopCategories>.Instance == null || Singleton<ShopCategories>.Instance.ShopUIOptions == null || Singleton<ShopCategories>.Instance.ShopUIOptions.ShowAvatar == null)
		{
			return true;
		}
		return Singleton<ShopCategories>.Instance.ShopUIOptions.ShowAvatar.IsValid(item);
	}
}
