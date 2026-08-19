using ItemSystem;

namespace Crafting;

public class RecipeSlot : IItemSlot
{
	public const string ModifyBaseSlotName = "base";

	public string Id;

	public string Name;

	public TagFilter[] RequiredTags;

	public TagFilter[] RequiredMaterials;

	public int CountMin;

	public int CountMax;

	public bool IsModifyBase => Id == "base";

	public string LocalizedName => Name;

	public bool IsSuitableItem(ItemData itemData, bool ignoreLevel = false)
	{
		return !itemData.IsEquipments && itemData.HasTagsAndMaterials(RequiredTags, RequiredMaterials, ignoreLevel);
	}
}
