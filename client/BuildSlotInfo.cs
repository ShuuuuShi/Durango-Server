using System.Collections.Generic;
using Building;
using Durango.Logic.Item;
using Messages;
using Yaml;

public class BuildSlotInfo : SlotInfo
{
	private readonly Building.BlueprintSlot _slot;

	private readonly List<ItemData> _previouslyAssignedItems = new List<ItemData>();

	private int _previouslyAssignedDummyCount;

	private int _maxCountModifier;

	public override int PreviouslyAssignedItemsCount => _previouslyAssignedDummyCount + PreviouslyAssignedItems.Count;

	public override IList<ItemData> PreviouslyAssignedItems => _previouslyAssignedItems;

	public override string Id => _slot.Id;

	public override OrTagFilter RequiredTags => _slot.AllowedTags;

	public override OrTagFilter RequiredMaterials => _slot.AllowedMaterials;

	public override SlotSourceInfo[] SlotSourceInfo => _slot.SlotSourceInfos;

	public override int Count => _slot.Count * _maxCountModifier;

	public override int RequiredLevel => _slot.RequiredLevel;

	public BuildSlotInfo(SlotContainer parent, Building.BlueprintSlot slot, int index, int maxCountModifier)
		: base(parent)
	{
		_slot = slot;
		_maxCountModifier = maxCountModifier;
		SetSlotInfo(index, _slot.Name);
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

	public override bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false)
	{
		return _slot.IsSuitableItem(itemData, ignoreSubReason);
	}
}
