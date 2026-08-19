using System.Collections.Generic;
using Crafting;
using ItemSystem;

public class CraftSlotInfo : SlotInfo
{
	private readonly RecipeSlot _slot;

	public override string Id => _slot.Id;

	public override IList<TagFilter> RequiredTags => _slot.RequiredTags;

	public override IList<TagFilter> RequiredMaterials => _slot.RequiredMaterials;

	public override int MaxCount => _slot.CountMin;

	public bool IsModifyBase => _slot.IsModifyBase;

	public CraftSlotInfo(RecipeSlot slot, int index)
	{
		_slot = slot;
		SetSlotInfo(index, _slot.LocalizedName, _slot.RequiredTags, _slot.RequiredMaterials);
		RefreshItemCount();
	}

	public override bool IsSuitableItem(ItemData itemData, bool ignoreLevel = false)
	{
		return _slot.IsSuitableItem(itemData, ignoreLevel);
	}
}
