using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

public abstract class SlotContainer
{
	protected readonly HashSet<string> ItemIDsHashSet = new HashSet<string>();

	private RecipeToolInfo _tool;

	private readonly BipartiteMatching _bipartiteMatching = new BipartiteMatching();

	private readonly List<ItemData> _priorityItemList = new List<ItemData>();

	public RecipeToolInfo Tool
	{
		get
		{
			if (_tool == null)
			{
				_tool = new RecipeToolInfo(this);
			}
			return _tool;
		}
	}

	public virtual int SlotCount => GetSlotCountExceptTool() + (Tool.ToolRequired ? 1 : 0);

	public SlotInfo CurrentSlot { get; protected set; }

	public abstract IList<ItemData> Items { get; }

	public virtual int Quantity => 1;

	public bool IsInit { get; private set; }

	public event Action<int> SlotChanged;

	public event Action SlotMaterialUpdated;

	public event Action Initialized;

	public event Action QuantityChanged;

	public virtual SlotInfo GetSlotInfo(int index)
	{
		int slotCountExceptTool = GetSlotCountExceptTool();
		if (0 <= index && index < slotCountExceptTool)
		{
			return GetSlotInfoExceptTool(index);
		}
		if (index == slotCountExceptTool && Tool.ToolRequired)
		{
			return Tool;
		}
		return null;
	}

	protected abstract void SlotItemSelectionUpdated();

	protected abstract SlotInfo GetSlotInfoExceptTool(int index);

	protected abstract int GetSlotCountExceptTool();

	public void SetCurrentSlotIndex(int index)
	{
		int obj = ((CurrentSlot == null) ? (-1) : CurrentSlot.Index);
		CurrentSlot = GetSlotInfo(index);
		if (this.SlotChanged != null)
		{
			this.SlotChanged(obj);
		}
	}

	public HashSet<string> GatherOtherSlotsSelectedItemIds(SlotInfo except = null)
	{
		HashSet<string> itemIDsHashSet = ItemIDsHashSet;
		itemIDsHashSet.Clear();
		for (int i = 0; i < SlotCount; i++)
		{
			SlotInfo slotInfo = GetSlotInfo(i);
			if (slotInfo != null && slotInfo != except)
			{
				for (int j = 0; j < slotInfo.SelectedItems.Count; j++)
				{
					ItemData itemData = slotInfo.SelectedItems[j];
					itemIDsHashSet.Add(itemData.Id);
				}
			}
		}
		return itemIDsHashSet;
	}

	[CanBeNull]
	public ItemData GetSafestItem()
	{
		ItemData itemData = null;
		int slotCountExceptTool = GetSlotCountExceptTool();
		for (int i = 0; i < slotCountExceptTool; i++)
		{
			SlotInfo slotInfo = GetSlotInfo(i);
			if (slotInfo != null && slotInfo.SafestItem != null && (itemData == null || itemData.SafeLevel < slotInfo.SafestItem.SafeLevel))
			{
				itemData = slotInfo.SafestItem;
			}
		}
		if (Tool.ToolRequired)
		{
			ItemData selectedItem = Tool.GetSelectedItem();
			if (selectedItem != null && (itemData == null || itemData.SafeLevel < selectedItem.SafeLevel))
			{
				itemData = selectedItem;
			}
		}
		return itemData;
	}

	public float GetAverageMaterialsLevel(int index)
	{
		float num = 0f;
		int num2 = 0;
		int i = 0;
		for (int slotCountExceptTool = GetSlotCountExceptTool(); i < slotCountExceptTool; i++)
		{
			SlotInfo slotInfo = GetSlotInfo(i);
			int j = index * slotInfo.Count;
			for (int num3 = Mathf.Min(slotInfo.SelectedItems.Count, (index + 1) * slotInfo.Count); j < num3; j++)
			{
				num += (float)slotInfo.SelectedItems[j].Level;
				num2++;
			}
		}
		if (num2 > 0)
		{
			return num / (float)num2;
		}
		return 0f;
	}

	protected void OnInit()
	{
		if (!IsInit)
		{
			IsInit = true;
			Tool.ItemListUpdated += SlotItemListUpdated;
			if (this.Initialized != null)
			{
				this.Initialized();
			}
		}
	}

	protected void ClearSlots()
	{
		int i = 0;
		for (int slotCountExceptTool = GetSlotCountExceptTool(); i < slotCountExceptTool; i++)
		{
			GetSlotInfo(i).ItemListUpdated -= SlotItemListUpdated;
		}
		OnClearSlot();
	}

	protected abstract void OnClearSlot();

	protected void AddSlot(SlotInfo slot)
	{
		slot.ItemListUpdated += SlotItemListUpdated;
		OnAddSlot(slot);
	}

	protected abstract void OnAddSlot(SlotInfo slot);

	[CanBeNull]
	public Dictionary<string, string[]> CreateFirstMaterialsDictionary(bool canFinished)
	{
		return CreateMaterialsDictionary(0, canFinished);
	}

	[CanBeNull]
	public Dictionary<string, string[]> CreateMaterialsDictionary(int index, bool canFinished)
	{
		return CreateMaterialItemsDictionary(index, (ItemData data) => data.Id, canFinished);
	}

	[CanBeNull]
	public Dictionary<string, ItemData[]> CreateMaterialItemsDictionary(int index, bool canFinished)
	{
		return CreateMaterialItemsDictionary(index, (ItemData data) => data, canFinished);
	}

	[CanBeNull]
	private Dictionary<string, T[]> CreateMaterialItemsDictionary<T>(int index, [NotNull] Func<ItemData, T> selector, bool canFinished)
	{
		if (Quantity <= index)
		{
			return null;
		}
		int slotCountExceptTool = GetSlotCountExceptTool();
		if (canFinished)
		{
			for (int i = 0; i < slotCountExceptTool; i++)
			{
				SlotInfo slotInfoExceptTool = GetSlotInfoExceptTool(i);
				int num = slotInfoExceptTool.Count * index;
				if (slotInfoExceptTool.Count > slotInfoExceptTool.SelectedItems.Count - num)
				{
					return null;
				}
			}
		}
		Dictionary<string, T[]> dictionary = new Dictionary<string, T[]>();
		for (int j = 0; j < slotCountExceptTool; j++)
		{
			SlotInfo slotInfoExceptTool2 = GetSlotInfoExceptTool(j);
			int num2 = slotInfoExceptTool2.Count * index;
			T[] array = new T[Mathf.Min(slotInfoExceptTool2.Count, slotInfoExceptTool2.SelectedItems.Count - num2)];
			for (int k = 0; k < array.Length; k++)
			{
				array[k] = selector(slotInfoExceptTool2.SelectedItems[k + num2]);
			}
			dictionary.Add(slotInfoExceptTool2.Id, array);
		}
		return dictionary;
	}

	public List<TagData> CreateMaterialsTags()
	{
		List<TagData> list = new List<TagData>();
		int slotCountExceptTool = GetSlotCountExceptTool();
		for (int i = 0; i < slotCountExceptTool; i++)
		{
			SlotInfo slotInfoExceptTool = GetSlotInfoExceptTool(i);
			int count = slotInfoExceptTool.SelectedItems.Count;
			for (int j = 0; j < count; j++)
			{
				list.AddRange(slotInfoExceptTool.SelectedItems[j].Tags);
			}
		}
		return list;
	}

	public string GetToolItemId()
	{
		if (Tool.ToolRequired)
		{
			ItemData selectedItem = Tool.GetSelectedItem();
			if (selectedItem != null)
			{
				return selectedItem.Id;
			}
		}
		return string.Empty;
	}

	public void OnSlotMaterialUpdate()
	{
		if (this.SlotMaterialUpdated != null)
		{
			this.SlotMaterialUpdated();
		}
	}

	protected void GetSlotCanQuickFillFlag(SlotInfo slotInfo, ref bool canQuickFill)
	{
		if (canQuickFill || slotInfo.State == SlotInfo.SlotState.FullSelected)
		{
			return;
		}
		IList<ItemData> items = Items;
		int i = 0;
		for (int size = KUtility.GetSize(items); i < size; i++)
		{
			if (!ItemIDsHashSet.Contains(items[i].Id) && slotInfo.IsSuitableItem(items[i]))
			{
				canQuickFill = true;
				break;
			}
		}
	}

	public void QuickFill()
	{
		_priorityItemList.Clear();
		if (Items != null)
		{
			_priorityItemList.AddRange(Items);
		}
		GatherOtherSlotsSelectedItemIds();
		for (int num = _priorityItemList.Count - 1; num >= 0; num--)
		{
			if (ItemIDsHashSet.Contains(_priorityItemList[num].Id))
			{
				_priorityItemList.RemoveAt(num);
			}
		}
		_priorityItemList.Sort(ItemPriorityComparison);
		_bipartiteMatching.Reset();
		int num2 = 0;
		for (int i = 0; i < SlotCount; i++)
		{
			SlotInfo slotInfo = GetSlotInfo(i);
			if (slotInfo == null)
			{
				continue;
			}
			int num3 = slotInfo.TotalCount - slotInfo.CurrentCount;
			if (num3 <= 0)
			{
				continue;
			}
			for (int j = 0; j < _priorityItemList.Count; j++)
			{
				if (slotInfo.IsSuitableItem(_priorityItemList[j]))
				{
					for (int k = 0; k < num3; k++)
					{
						_bipartiteMatching.SetLink(num2 + k, j);
					}
				}
			}
			num2 += num3;
		}
		_bipartiteMatching.Match();
		num2 = 0;
		for (int l = 0; l < SlotCount; l++)
		{
			SlotInfo slotInfo2 = GetSlotInfo(l);
			if (slotInfo2 == null)
			{
				continue;
			}
			int num4 = slotInfo2.TotalCount - slotInfo2.CurrentCount;
			for (int num5 = num4 - 1; num5 >= 0; num5--)
			{
				int index = num2 + num5;
				int link = _bipartiteMatching.GetLink(index);
				if (link != -1)
				{
					slotInfo2.AddSelectedItem(_priorityItemList[link]);
				}
			}
			num2 += num4;
		}
		SlotItemSelectionUpdated();
	}

	public abstract int ItemPriorityComparison(ItemData i1, ItemData i2);

	private void SlotItemListUpdated()
	{
		SlotItemSelectionUpdated();
	}

	public List<ItemData> GetSelectedMaterials()
	{
		List<ItemData> list = null;
		if (IsInit)
		{
			list = new List<ItemData>();
			for (int i = 0; i < SlotCount; i++)
			{
				SlotInfo slotInfo = GetSlotInfo(i);
				if (slotInfo != null)
				{
					list.AddRange(slotInfo.SelectedItems);
				}
			}
		}
		return list;
	}

	public virtual void SetQuantity(int value)
	{
	}

	protected void OnQuantityChanged()
	{
		for (int i = 0; i < SlotCount; i++)
		{
			GetSlotInfo(i).CheckSelectedItems();
		}
		SlotItemSelectionUpdated();
		if (this.QuantityChanged != null)
		{
			this.QuantityChanged();
		}
	}

	public abstract int CalcMaxQuantity();
}
public abstract class SlotContainer<T> : SlotContainer where T : SlotInfo
{
	protected readonly List<T> Slots = new List<T>();

	protected override SlotInfo GetSlotInfoExceptTool(int index)
	{
		return Slots[index];
	}

	protected override int GetSlotCountExceptTool()
	{
		return Slots.Count;
	}

	protected override void OnClearSlot()
	{
		Slots.Clear();
	}

	protected override void OnAddSlot(SlotInfo slot)
	{
		if (slot is T item)
		{
			Slots.Add(item);
		}
	}
}
