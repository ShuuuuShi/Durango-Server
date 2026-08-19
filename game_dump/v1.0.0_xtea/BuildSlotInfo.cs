using System.Collections.Generic;
using Building_;
using ItemSystem;
using Messages;

public class BuildSlotInfo : SlotInfo
{
	private readonly BlueprintSlot _slot;

	private readonly List<ItemData> _previouslyAssignedItems = new List<ItemData>();

	private int _previouslyAssignedDummyCount;

	private int _maxCountModifier;

	public override string Id => _slot.Id;

	public override IList<TagFilter> RequiredTags => _slot.RequiredTags;

	public override IList<TagFilter> RequiredMaterials => _slot.RequiredMaterials;

	public override int MaxCount => _slot.RequiredCount * _maxCountModifier;

	public override int PreviouslyAssignedItemsCount => _previouslyAssignedDummyCount + PreviouslyAssignedItems.Count;

	public override IList<ItemData> PreviouslyAssignedItems => _previouslyAssignedItems;

	public BuildSlotInfo(BlueprintSlot slot, int index, int maxCountModifier)
	{
		_slot = slot;
		_maxCountModifier = maxCountModifier;
		SetSlotInfo(index, _slot.LocalizedName, _slot.RequiredTags, _slot.RequiredMaterials);
		SetPrevMaterials(null);
	}

	public void SetPrevMaterials(IList<Item> previouslyItems)
	{
		_previouslyAssignedItems.Clear();
		if (previouslyItems != null)
		{
			int i = 0;
			for (int count = previouslyItems.Count; i < count; i++)
			{
				_previouslyAssignedItems.Add(new ItemData(previouslyItems[i]));
			}
		}
		OnUpdateItemList();
	}

	public void SetPrevAssignedItemsDummyCount(int dummyCount)
	{
		_previouslyAssignedDummyCount = dummyCount;
		OnUpdateItemList();
	}

	public override bool IsSuitableItem(ItemData itemData, bool ignoreLevel = false)
	{
		return _slot.IsSuitableItem(itemData, ignoreLevel);
	}
}
