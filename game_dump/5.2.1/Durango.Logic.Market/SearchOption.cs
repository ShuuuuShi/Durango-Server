using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Utils.Extensions;
using Messages;
using Shared.Economy;

namespace Durango.Logic.Market;

public class SearchOption
{
	public Category.Main MainCategory;

	public Category.Sub SubCategory;

	public string SearchKeyword;

	public PriceRangePredicate Price;

	public RangePredicate Level;

	public string Prototype;

	public HashSet<TagFilterBase> Tags = new HashSet<TagFilterBase>(new TagFilterComparer());

	public HashSet<TagFilterBase> Materials = new HashSet<TagFilterBase>(new TagFilterComparer());

	public bool IsSlotSearch
	{
		get
		{
			if (Tags.Count == 0)
			{
				return Materials.Count != 0;
			}
			return true;
		}
	}

	public bool IsResultSearch
	{
		get
		{
			if (MainCategory != null)
			{
				return !string.IsNullOrEmpty(SearchKeyword);
			}
			return false;
		}
	}

	public bool Filter(string name)
	{
		if (IsResultSearch)
		{
			return name.ContainsIgnoreCase(SearchKeyword);
		}
		return true;
	}

	public void Clear()
	{
		MainCategory = null;
		SubCategory = null;
		SearchKeyword = null;
		Price.Min = null;
		Price.Max = null;
		Price.Currency = Currency.TStone;
		Level.Min = null;
		Level.Max = null;
		Prototype = null;
		Tags.Clear();
		Materials.Clear();
	}

	public void ClearExceptCategory()
	{
		Category.Main mainCategory = MainCategory;
		Category.Sub subCategory = SubCategory;
		Clear();
		MainCategory = mainCategory;
		SubCategory = subCategory;
	}

	public string[][] GetNestedTag()
	{
		string[][] array = new string[Tags.Count + Materials.Count][];
		int num = 0;
		foreach (TagFilterBase tag in Tags)
		{
			array[num++] = tag.GetIdArray();
		}
		foreach (TagFilterBase material in Materials)
		{
			array[num++] = material.GetIdArray();
		}
		return array;
	}

	public SearchProducts ToMessage()
	{
		SearchProducts result = default(SearchProducts);
		result.ItemName = SearchKeyword;
		result.PrototypeId = Prototype;
		result.NestedTags = GetNestedTag();
		result.Category = ((MainCategory == null) ? null : MainCategory.Id);
		result.SubCategories = ((SubCategory == null) ? new string[0] : new string[1] { SubCategory.Id });
		result.Price = ((!Price.Min.HasValue && !Price.Max.HasValue) ? null : new PriceRangePredicate?(Price));
		result.Level = ((!Level.Min.HasValue && !Level.Max.HasValue) ? null : new RangePredicate?(Level));
		return result;
	}
}
