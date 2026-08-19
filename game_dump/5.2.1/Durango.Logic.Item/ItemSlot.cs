using Yaml;

namespace Durango.Logic.Item;

public class ItemSlot
{
	public string Id { get; set; }

	public string Name { get; set; }

	public int Count { get; set; }

	public int RequiredLevel { get; set; }

	public OrTagFilter AllowedTags { get; set; }

	public OrTagFilter AllowedMaterials { get; set; }

	public SlotSourceInfo[] SlotSourceInfos { get; set; }

	public virtual bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)
	{
		if (!itemData.IsDestroyed() && !itemData.IsEquipments)
		{
			return itemData.HasTagsAndMaterials(AllowedTags, AllowedMaterials, ignoreSubReason);
		}
		return false;
	}
}
