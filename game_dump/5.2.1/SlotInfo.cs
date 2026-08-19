using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using JetBrains.Annotations;
using Yaml;

public abstract class SlotInfo
{
	public enum SlotState
	{
		NoSelected,
		SomeSelected,
		FullSelected
	}

	private static readonly ItemData[] Empty = new ItemData[0];

	private readonly List<ItemData> _selectedItems = new List<ItemData>();

	public abstract string Id { get; }

	public abstract OrTagFilter RequiredTags { get; }

	public abstract OrTagFilter RequiredMaterials { get; }

	public abstract SlotSourceInfo[] SlotSourceInfo { get; }

	public abstract int Count { get; }

	public virtual int TotalCount => Count * Parent.Quantity;

	public abstract int RequiredLevel { get; }

	public SlotContainer Parent { get; private set; }

	public int Index { get; private set; }

	public ItemData SafestItem { get; private set; }

	public string Name { get; private set; }

	public virtual int CurrentCount => PreviouslyAssignedItemsCount + _selectedItems.Count;

	public SlotState State
	{
		get
		{
			if (CurrentCount >= TotalCount)
			{
				return SlotState.FullSelected;
			}
			if (_selectedItems.Count <= 0)
			{
				return SlotState.NoSelected;
			}
			return SlotState.SomeSelected;
		}
	}

	[NotNull]
	public virtual IList<ItemData> SelectedItems => _selectedItems;

	public virtual IList<ItemData> PreviouslyAssignedItems => Empty;

	public virtual int PreviouslyAssignedItemsCount => Empty.Length;

	public event Action ItemListUpdated;

	protected SlotInfo(SlotContainer parent)
	{
		Parent = parent;
	}

	public void AddSelectedItem(ItemData itemData)
	{
		_selectedItems.Add(itemData);
		if (SafestItem == null || SafestItem.SafeLevel < itemData.SafeLevel)
		{
			SafestItem = itemData;
		}
	}

	public void SetSelectedItems(IList<ItemData> list)
	{
		_selectedItems.Clear();
		SafestItem = null;
		int totalCount = TotalCount;
		for (int i = 0; i < list.Count; i++)
		{
			ItemData itemData = list[i];
			_selectedItems.Add(itemData);
			if (SafestItem == null || SafestItem.SafeLevel < itemData.SafeLevel)
			{
				SafestItem = itemData;
			}
			if (_selectedItems.Count >= totalCount)
			{
				break;
			}
		}
		OnUpdateItemList();
	}

	public abstract bool IsSuitableItem(ItemData itemData, bool ignoreSubReason = false);

	public void OnUpdateItemList()
	{
		if (this.ItemListUpdated != null)
		{
			this.ItemListUpdated();
		}
	}

	protected void SetSlotInfo(int index, string textName)
	{
		Index = index;
		Name = textName;
	}

	public void CheckSelectedItems()
	{
		int totalCount = TotalCount;
		if (CurrentCount >= totalCount)
		{
			_selectedItems.RemoveRange(totalCount, SelectedItems.Count - totalCount);
		}
	}
}
