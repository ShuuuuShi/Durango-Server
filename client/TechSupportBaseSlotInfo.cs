using System.Collections.Generic;
using Crafting;
using Durango.Logic.Item;

public class TechSupportBaseSlotInfo : CraftSlotInfo
{
	private readonly ItemData[] _fixedItems;

	public override int Count => _fixedItems.Length;

	public override int CurrentCount => _fixedItems.Length;

	public override IList<ItemData> SelectedItems => _fixedItems;

	public TechSupportTarget Target { get; private set; }

	public TechSupportBaseSlotInfo(SlotContainer parent, RecipeSlot slot, int index, TechSupportTarget techSupportTarget)
		: base(parent, slot, index)
	{
		Target = techSupportTarget;
		_fixedItems = ((Target.Item == null) ? new ItemData[0] : new ItemData[1] { Target.Item });
	}

	public override bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)
	{
		return Target.Item != null && Target.Item.Id == itemData.Id;
	}
}
