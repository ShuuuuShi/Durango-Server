using System.Collections.Generic;
using System.Text;
using Building;
using Crafting;
using Durango.Logic.Item;
using Durango.Logic.Statistics;
using Durango.Utils;
using L10N;
using Newtonsoft.Json;
using Shared.Economy;
using Yaml.Util;

namespace Yaml;

public class WarpRushReward
{
	public class CurrencyInfo
	{
		[JsonProperty(PropertyName = "currency_amount")]
		public int Amount;

		[JsonProperty(PropertyName = "currency_type")]
		public Currency Type;
	}

	public class ItemInfo
	{
		[JsonProperty(PropertyName = "count")]
		public int Count;

		[JsonProperty(PropertyName = "level")]
		public int Level;

		[JsonProperty(PropertyName = "prototype_id")]
		public string PrototypeId;

		[JsonProperty(PropertyName = "default_tags")]
		public Dictionary<string, int> DefaultTags;

		[JsonProperty(PropertyName = "random_tags")]
		public Dictionary<string, int> RandomTags;

		public Dictionary<string, int> RareTags;
	}

	public class VoucherInfo
	{
		public string Id;

		public int Count;
	}

	[JsonProperty(PropertyName = "currency")]
	public CurrencyInfo Currency;

	[JsonProperty(PropertyName = "item")]
	public ItemInfo Item;

	[JsonProperty(PropertyName = "recipe_id")]
	public string Recipe;

	[JsonProperty(PropertyName = "blueprint_id")]
	public string BlueprintId;

	[JsonIgnore]
	public string CommodityId;

	public string Title;

	public VoucherInfo Voucher;

	public int GetCount()
	{
		if (Currency != null)
		{
			return Currency.Amount;
		}
		if (Item != null)
		{
			return Item.Count;
		}
		if (Voucher != null)
		{
			return Voucher.Count;
		}
		return 0;
	}

	public int GetLevel()
	{
		if (Item == null)
		{
			return 0;
		}
		return Item.Level;
	}

	public void GetTooltip(out string title, out string comment)
	{
		title = null;
		comment = null;
		if (Currency != null)
		{
			title = $"[icon={Inventory.GetIcon(Currency.Type)}] {GetCount()}";
		}
		else
		{
			if (Item != null)
			{
				Prototype itemPrototype = PrototypeYaml.GetItemPrototype(Item.PrototypeId, Item.Level);
				if (itemPrototype == null)
				{
					return;
				}
				title = T._("{0} {1:lv:}", itemPrototype.Name, Item.Level);
				using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
				StringBuilder value = reusable.Value;
				if (KUtility.GetSize(Item.DefaultTags) > 0)
				{
					value.AppendFormat("{0}: {1}\n", T._("기본 속성"), Durango.Logic.Item.Util.LocalizedTagNamesAndLevels(Item.DefaultTags));
				}
				if (KUtility.GetSize(Item.RandomTags) > 0)
				{
					value.AppendFormat("{0}: {1}\n", T._("랜덤 잠재속성"), Durango.Logic.Item.Util.LocalizedTagNamesAndLevels(Item.RandomTags));
				}
				if (KUtility.GetSize(Item.RareTags) > 0)
				{
					value.AppendFormat("{0}: {1}\n", T._("희귀 속성"), Durango.Logic.Item.Util.LocalizedTagNamesAndLevels(Item.RareTags));
				}
				comment = value.ToString().Trim();
				return;
			}
			if (!string.IsNullOrEmpty(Recipe))
			{
				Crafting.Recipe recipe = GameSystem<RecipeSystem>.Instance().GetRecipe(Recipe);
				if (recipe != null)
				{
					title = recipe.Name;
					comment = string.Format("{0}: {1}", T._("제작법"), recipe.Description);
				}
			}
			else if (!string.IsNullOrEmpty(BlueprintId))
			{
				Building.Blueprint blueprint = GameSystem<RecipeSystem>.Instance().GetBlueprint(BlueprintId);
				if (blueprint != null)
				{
					title = blueprint.Name;
					comment = string.Format("{0}: {1}", T._("제작법"), blueprint.Description);
				}
			}
			else if (!string.IsNullOrEmpty(Title))
			{
				Durango.Logic.Statistics.Title title2 = GameSystem<StatisticsSystem>.Instance().GetTitle(Title);
				if (title2 != null)
				{
					title = title2.Name;
					comment = title2.Description;
				}
			}
			else if (Voucher != null)
			{
				Voucher voucher = SingletonDict<string, Yaml.Voucher>.Get(Voucher.Id);
				if (voucher.IsValid())
				{
					title = voucher.Name;
					comment = voucher.Description;
				}
			}
		}
	}
}
