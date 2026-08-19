using Durango.Logic.Shop;
using Durango.Utils.Extensions;
using Newtonsoft.Json;
using Shared.Purchaser;

namespace Yaml;

public class ShopCategoryCondition
{
	[JsonProperty(PropertyName = "tag")]
	public Tags? Tag;

	[JsonProperty(PropertyName = "type")]
	public CommodityType? Type;

	[JsonProperty(PropertyName = "ui_category")]
	public string Category;

	[JsonProperty(PropertyName = "commodity_ids")]
	public string[] CommodityIds;

	public bool IsValidCommodity(Durango.Logic.Shop.Commodity commodity)
	{
		if (KUtility.GetSize(CommodityIds) > 0 && CommodityIds.IndexOf(commodity.Id) == -1)
		{
			return false;
		}
		if (Tag.HasValue && (commodity.SalesTag & Tag.Value) == 0)
		{
			return false;
		}
		if (Type.HasValue && commodity.Data.Type != Type.Value)
		{
			return false;
		}
		if (!string.IsNullOrEmpty(Category) && commodity.Data.Category != Category)
		{
			return false;
		}
		return true;
	}
}
