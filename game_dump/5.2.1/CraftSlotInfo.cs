using Crafting;
using Durango.Logic.Item;
using Yaml;

public class CraftSlotInfo : SlotInfo
{
	private readonly Crafting.RecipeSlot _slot;

	public Crafting.RecipeSlot.Type SlotType => _slot.SlotType;

	public override string Id => _slot.Id;

	public override OrTagFilter RequiredTags => _slot.AllowedTags;

	public override OrTagFilter RequiredMaterials => _slot.AllowedMaterials;

	public override SlotSourceInfo[] SlotSourceInfo => _slot.SlotSourceInfos;

	public override int Count => _slot.Count;

	public override int RequiredLevel => _slot.RequiredLevel;

	public CraftSlotInfo(SlotContainer parent, Crafting.RecipeSlot slot, int index)
		: base(parent)
	{
		_slot = slot;
		SetSlotInfo(index, _slot.Name);
	}

	public override bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)
	{
		return _slot.IsSuitableItem(itemData, ignoreSubReason);
	}
}
