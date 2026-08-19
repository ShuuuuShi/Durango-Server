using System;
using System.Collections.Generic;
using ItemSystem;
using JetBrains.Annotations;

public abstract class SlotContainer
{
	protected readonly HashSet<ulong> ItemIDsHashSet = new HashSet<ulong>();

	public abstract int SlotCount { get; }

	public SlotInfo CurrentSlot { get; protected set; }

	public event Action<int> SlotChanged;

	public abstract SlotInfo GetSlotInfo(int index);

	protected abstract void SlotItemSelectionUpdated();

	public void SetCurrentSlotIndex(int index)
	{
		int obj = ((CurrentSlot == null) ? (-1) : CurrentSlot.Index);
		CurrentSlot = GetSlotInfo(index);
		if (this.SlotChanged != null)
		{
			this.SlotChanged(obj);
		}
	}

	public void DisableAlreadySelectedItemIconsByOtherSlots(List<ItemIcon2> itemIcons, Action<ulong> enumerationDisabledIds)
	{
		GatherOtherSlotsSelectedItemIds(ItemIDsHashSet, CurrentSlot);
		for (int i = 0; i < itemIcons.Count; i++)
		{
			ItemIcon2 itemIcon = itemIcons[i];
			if (ItemIDsHashSet.Contains(itemIcon.Item.Id))
			{
				itemIcon.IconMode = ItemIcon2.Mode.DisableWithSelectionMark;
				enumerationDisabledIds(itemIcon.Item.Id);
			}
		}
	}

	protected void GatherOtherSlotsSelectedItemIds(HashSet<ulong> hashSet, SlotInfo except = null)
	{
		hashSet.Clear();
		for (int i = 0; i < SlotCount; i++)
		{
			SlotInfo slotInfo = GetSlotInfo(i);
			if (slotInfo != null && slotInfo != except)
			{
				for (int j = 0; j < slotInfo.SelectedItems.Count; j++)
				{
					ItemData itemData = slotInfo.SelectedItems[j];
					hashSet.Add(itemData.Id);
				}
			}
		}
	}

	protected void SlotQuickFill(SlotInfo slotInfo, [NotNull] IList<ItemData> items)
	{
		if (slotInfo.State != SlotInfo.SlotState.FullSelected)
		{
			GatherOtherSlotsSelectedItemIds(ItemIDsHashSet);
			List<ItemData> list = Util.Filtering(items, (ItemData itemData) => !ItemIDsHashSet.Contains(itemData.Id) && !itemData.Like && slotInfo.IsSuitableItem(itemData));
			slotInfo.AddSelectedItems(list);
		}
	}
}
public abstract class SlotContainer<T> : SlotContainer where T : SlotInfo
{
	protected List<T> _slots = new List<T>();

	protected RecipeToolInfo _tool = new RecipeToolInfo();

	protected ExpectedResultInfo _expectedResultInfo = new ExpectedResultInfo();

	public RecipeToolInfo Tool => _tool;

	public abstract IList<ItemData> Items { get; }

	public bool IsInit { get; private set; }

	public IExpectedResultInfo ExpectedResult => (!_expectedResultInfo.IsValid) ? null : _expectedResultInfo;

	public override int SlotCount => _slots.Count + (_tool.ToolRequired ? 1 : 0);

	public event Action<SlotContainer> ExpectedResultUpdated;

	public event Action<SlotContainer> Initialized;

	public event Action<SlotContainer> Disposed;

	public override SlotInfo GetSlotInfo(int index)
	{
		if (0 <= index && index < _slots.Count)
		{
			return _slots[index];
		}
		if (index == _slots.Count && _tool.ToolRequired)
		{
			return _tool;
		}
		return null;
	}

	public bool HasLockedItem()
	{
		for (int i = 0; i < _slots.Count; i++)
		{
			T val = _slots[i];
			if (val.HasLockedItem)
			{
				return true;
			}
		}
		return false;
	}

	public float GetAverageMaterialsLevel()
	{
		float num = 0f;
		int num2 = 0;
		for (int i = 0; i < _slots.Count; i++)
		{
			T val = _slots[i];
			for (int j = 0; j < val.SelectedItems.Count; j++)
			{
				num += (float)val.SelectedItems[j].Level;
				num2++;
			}
		}
		return (num2 <= 0) ? 0f : (num / (float)num2);
	}

	public void Dispose()
	{
		if (IsInit)
		{
			IsInit = false;
			OnDispose();
			if (this.Disposed != null)
			{
				this.Disposed(this);
			}
		}
	}

	protected virtual void OnDispose()
	{
		_slots.Clear();
		_tool.Clear();
		_expectedResultInfo.Clear();
	}

	protected void OnInit()
	{
		if (!IsInit)
		{
			IsInit = true;
			_tool.ItemListUpdated += slot_ItemListUpdated;
			if (this.Initialized != null)
			{
				this.Initialized(this);
			}
		}
	}

	protected void ClearSlots()
	{
		int i = 0;
		for (int count = _slots.Count; i < count; i++)
		{
			T val = _slots[i];
			val.ItemListUpdated -= slot_ItemListUpdated;
		}
		_slots.Clear();
	}

	protected void AddSlot(T slot)
	{
		slot.ItemListUpdated += slot_ItemListUpdated;
		_slots.Add(slot);
	}

	public void QuickFill()
	{
		QuickFill(Items);
	}

	public Dictionary<string, ulong[]> CreateMaterialsDictionary()
	{
		Dictionary<string, ulong[]> dictionary = new Dictionary<string, ulong[]>();
		for (int i = 0; i < _slots.Count; i++)
		{
			T val = _slots[i];
			ulong[] array = Util.ItemsToIds(val.SelectedItems);
			if (array == null)
			{
				array = new ulong[0];
			}
			dictionary.Add(val.Id, array);
		}
		return dictionary;
	}

	public List<TagData> CreateMaterialsTags()
	{
		List<TagData> list = new List<TagData>();
		for (int i = 0; i < _slots.Count; i++)
		{
			T val = _slots[i];
			int count = val.SelectedItems.Count;
			for (int j = 0; j < count; j++)
			{
				list.AddRange(val.SelectedItems[j].Tags);
			}
		}
		return list;
	}

	public ulong? GetToolItemId()
	{
		if (_tool.ToolRequired)
		{
			ItemData selectedItem = _tool.GetSelectedItem();
			if (selectedItem != null)
			{
				return selectedItem.Id;
			}
		}
		return null;
	}

	protected void GetSlotCanQuickFillFlag(SlotInfo slotInfo, ref bool canQuickFill)
	{
		if (!canQuickFill && slotInfo.State != SlotInfo.SlotState.FullSelected)
		{
			IList<ItemData> items = Items;
			GatherOtherSlotsSelectedItemIds(ItemIDsHashSet);
			canQuickFill = Util.Exist(items, (ItemData itemData) => !ItemIDsHashSet.Contains(itemData.Id) && !itemData.Like && slotInfo.IsSuitableItem(itemData));
		}
	}

	protected void OnUpdateExpectedResult()
	{
		if (this.ExpectedResultUpdated != null)
		{
			this.ExpectedResultUpdated(this);
		}
	}

	private void QuickFill(IList<ItemData> items)
	{
		IItemSlot[] itemSlots = new IItemSlot[_slots.Count];
		for (int i = 0; i < itemSlots.Length; i++)
		{
			itemSlots[i] = _slots[i];
		}
		List<ItemData> list = new List<ItemData>(items);
		list.Sort(delegate(ItemData x, ItemData y)
		{
			int num = Util.GetSlotCountBySuitableItem(x, itemSlots) - Util.GetSlotCountBySuitableItem(y, itemSlots);
			return (num != 0) ? num : (y.ModifiableCount - x.ModifiableCount);
		});
		for (int j = 0; j < SlotCount; j++)
		{
			SlotInfo slotInfo = GetSlotInfo(j);
			if (slotInfo != null)
			{
				SlotQuickFill(slotInfo, list);
			}
		}
		SlotItemSelectionUpdated();
	}

	private void slot_ItemListUpdated()
	{
		SlotItemSelectionUpdated();
	}
}
