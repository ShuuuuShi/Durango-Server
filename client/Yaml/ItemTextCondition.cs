using Durango.Logic.Item;
using Durango.Utils.Extensions;
using Newtonsoft.Json;

namespace Yaml;

public class ItemTextCondition
{
	[JsonProperty(PropertyName = "item_category")]
	public string[] ItemCategory;

	public bool IsValid(ItemData item)
	{
		if (ItemCategory == null)
		{
			return false;
		}
		if (item.Prototype == null)
		{
			return false;
		}
		return ItemCategory.IndexOf(item.Prototype.Category) != -1;
	}
}
