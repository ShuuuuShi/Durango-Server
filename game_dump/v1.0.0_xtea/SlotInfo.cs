using System;
using System.Collections.Generic;
using System.Text;
using ItemSystem;
using UnityEngine;
using Yaml;
using Yaml.Util;

public abstract class SlotInfo : IItemSlot
{
	public enum SlotState
	{
		NoSelected,
		SomeSelected,
		FullSelected
	}

	private static readonly ItemData[] _empty = new ItemData[0];

	private readonly List<ItemData> _selectedItems = new List<ItemData>();

	public abstract string Id { get; }

	public abstract IList<TagFilter> RequiredTags { get; }

	public abstract IList<TagFilter> RequiredMaterials { get; }

	public abstract int MaxCount { get; }

	public int Index { get; private set; }

	public bool HasLockedItem { get; private set; }

	public string TextName { get; private set; }

	public string TextRequiredTags { get; private set; }

	public string TextRequiredMaterials { get; private set; }

	public string IconName { get; private set; }

	public int TagLevel { get; private set; }

	public string TextCount { get; private set; }

	public int CurrentCount => PreviouslyAssignedItemsCount + SelectedItems.Count;

	public SlotState State
	{
		get
		{
			if (CurrentCount >= MaxCount)
			{
				return SlotState.FullSelected;
			}
			return (_selectedItems.Count > 0) ? SlotState.SomeSelected : SlotState.NoSelected;
		}
	}

	public IList<ItemData> SelectedItems => _selectedItems;

	public virtual IList<ItemData> PreviouslyAssignedItems => _empty;

	public virtual int PreviouslyAssignedItemsCount => _empty.Length;

	public event Action ItemListUpdated;

	public void AddSelectedItems(IList<ItemData> list)
	{
		int num = Mathf.Min(list.Count, MaxCount - CurrentCount);
		for (int i = 0; i < num; i++)
		{
			ItemData itemData = list[i];
			_selectedItems.Add(itemData);
			if (itemData.Like)
			{
				HasLockedItem = true;
			}
		}
		OnUpdateItemList();
	}

	public void SetSelectedItems(IList<ItemIcon2> list)
	{
		_selectedItems.Clear();
		HasLockedItem = false;
		for (int i = 0; i < list.Count; i++)
		{
			ItemIcon2 itemIcon = list[i];
			if (itemIcon.IconMode == ItemIcon2.Mode.Enable)
			{
				_selectedItems.Add(itemIcon.Item);
				if (itemIcon.Item.Like)
				{
					HasLockedItem = true;
				}
				if (_selectedItems.Count >= MaxCount)
				{
					break;
				}
			}
		}
		OnUpdateItemList();
	}

	public abstract bool IsSuitableItem(ItemData itemData, bool ignoreLevel = false);

	protected void OnUpdateItemList()
	{
		RefreshItemCount();
		if (this.ItemListUpdated != null)
		{
			this.ItemListUpdated();
		}
	}

	protected void SetSlotInfo(int index, string textName, IList<TagFilter> requiredTags, IList<TagFilter> requiredMaterials)
	{
		string iconName = string.Empty;
		int? level = null;
		Index = index;
		TextName = textName;
		TextRequiredTags = CreateTagText(RequiredTags, ref iconName, ref level);
		TextRequiredMaterials = CreateTagText(RequiredMaterials, ref iconName, ref level);
		IconName = iconName;
		TagLevel = (level.HasValue ? level.Value : 0);
	}

	protected void RefreshItemCount()
	{
		TextCount = $"{CurrentCount} / {MaxCount}";
	}

	public static string CreateTagText(IList<TagFilter> tags, ref string iconName, ref int? level)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < tags.Count; i++)
		{
			TagFilter tagFilter = tags[i];
			Tag tag = SingletonDict<string, Tag>.Get(tagFilter.TagId);
			if (tag != null)
			{
				if (iconName == string.Empty)
				{
					iconName = tag.icon;
				}
				if (!level.HasValue)
				{
					level = tagFilter.RequiredLevel;
				}
				if (stringBuilder.Length > 0)
				{
					stringBuilder.Append(", ");
				}
				stringBuilder.Append(tag.name);
			}
		}
		return stringBuilder.ToString();
	}
}
