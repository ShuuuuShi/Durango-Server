using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Utils.Extensions;
using Messages;
using Shared.Economy;
using Shared.Market;
using Yaml;
using Yaml.Util;

namespace Durango.Offline;

public class MarketManager
{
	private Product[] _products;

	private static readonly string[] Tags = new string[11]
	{
		"window", "wall_deco", "empty_door", "plantable", "armor", "weapon", "instrument", "wood", "burnable", "dried",
		"wet"
	};

	public Product[] Products
	{
		get
		{
			if (_products == null)
			{
				List<Product> list = new List<Product>();
				list.AddRange(SingletonDict<string, List<Prototype>>.Instance.Where(delegate(KeyValuePair<string, List<Prototype>> pair)
				{
					if (pair.Value == null)
					{
						return false;
					}
					_ = pair.Value;
					return true;
				}).Select(delegate(KeyValuePair<string, List<Prototype>> prototype)
				{
					KeyValuePair<string, List<Prototype>> keyValuePair = prototype;
					return MakeProduct(keyValuePair.Key);
				}));
				_products = list.ToArray();
			}
			return _products;
		}
	}

	private Product MakeProduct(string prototypeId)
	{
		Product result = default(Product);
		result.Id = Guid.NewGuid().ToString();
		result.RegionId = "1";
		result.ListedAt = 0.0;
		result.ExpiresAt = 0.0;
		result.DeletesAt = 0.0;
		result.PurchasedAt = null;
		result.Price = 0L;
		result.Fee = 0L;
		result.Currency = Currency.TStone;
		result.State = ProductState.Registered;
		result.Level = 60;
		result.Durability = 10000f;
		Item? item = Cheats.MakeItem(prototypeId, result.Level);
		if (item.HasValue)
		{
			result.Items = new Item[1] { item.Value };
		}
		return result;
	}

	public Item[] BuyProduct(string productId)
	{
		Product value = Products.FirstOrDefault((Product product) => product.Id == productId);
		if (string.IsNullOrEmpty(value.Id))
		{
			return null;
		}
		Item item = value.Items.FirstOrDefault();
		if (string.IsNullOrEmpty(item.Prototype))
		{
			return null;
		}
		int num = Products.IndexOf(value);
		Item? item2 = Cheats.MakeItem(item.Prototype, item.Level);
		if (item2.HasValue)
		{
			Products[num].Items = new Item[1] { item2.Value };
		}
		return value.Items;
	}

	public Products SearchProduct(SearchProducts option)
	{
		IEnumerable<Product> products = Products;
		products = products.Where((Product product) => product.Items != null && !string.IsNullOrEmpty(product.Items.FirstOrDefault(delegate(Item item)
		{
			if (!string.IsNullOrEmpty(option.ItemName) && !string.IsNullOrEmpty(item.Name) && !item.Name.Contains(option.ItemName))
			{
				return false;
			}
			Prototype prototype = PrototypeYaml.GetItemPrototype(item.Prototype);
			if (prototype == null)
			{
				return false;
			}
			if (KUtility.GetSize(option.SubCategories) > 0)
			{
				if (KUtility.GetSize(option.SubCategories) <= 0)
				{
					return false;
				}
				if (option.SubCategories.All((string subCategory) => !prototype.SubCategories.Any((string s) => s == subCategory)))
				{
					return false;
				}
			}
			return string.IsNullOrEmpty(option.Category) || string.IsNullOrEmpty(prototype.Category) || prototype.Category == option.Category;
		}).Id)).Skip(option.Skip).Take(OptionSystem.GetMarketSearchLimit());
		Products result = default(Products);
		result._Products = products.ToArray();
		return result;
	}
}
