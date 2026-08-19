using System;
using System.Collections.Generic;
using ItemSystem;
using L10N;
using UnityEngine;

public class MaterialSelectWidget : MonoBehaviour
{
	[SerializeField]
	private UILabel _textMaterialName;

	[SerializeField]
	private UILabel _textMaterialCount;

	[SerializeField]
	private UILabel _textTagLevel;

	[SerializeField]
	private UILabel _textRequiredTags;

	[SerializeField]
	private UILabel _labelMaterial;

	[SerializeField]
	private UILabel _textRequiredMaterials;

	[SerializeField]
	private ItemList _itemList;

	[SerializeField]
	private GameObject _materialInfoContainer;

	[SerializeField]
	private MaterialInfoWidget _infoPrefab;

	[SerializeField]
	private Color _colorTagLevelNormal;

	[SerializeField]
	private Color _colorTagLevelWarning;

	private bool _initialized;

	private SlotContainer _slotContainer;

	private MaterialInfoWidget _materialInfoWidget;

	private Dictionary<ulong, MaterialInfoWidget.WarningType> _warningDictionary = new Dictionary<ulong, MaterialInfoWidget.WarningType>();

	private bool _refreshingItemList;

	public event Action ItemSelectionUpdated;

	public void Set(SlotContainer slotContainer)
	{
		Init();
		_slotContainer = slotContainer;
	}

	public void Refresh()
	{
		RefreshUpperBar();
		RefreshItemList();
		RefreshMaterialInfo();
	}

	public void RepositionItemList()
	{
		_itemList.FixedIconSize = true;
		_itemList.ResetPosition();
	}

	public ItemIcon2 GetFirstSelectableEnabledItemOrNull()
	{
		return _itemList.GetFirstSelectableEnabledItemOrNull();
	}

	private void Init()
	{
		if (!_initialized)
		{
			_itemList.OnUpdateSelectItem = OnUpdateSelectItem;
			_materialInfoWidget = CreateMaterialInfoWidget(_materialInfoContainer, _infoPrefab);
			_materialInfoWidget.Init();
			UIUtility.SetLabelText(_labelMaterial, T._("재질"));
			_initialized = true;
		}
	}

	private void RefreshUpperBar()
	{
		if (_slotContainer != null)
		{
			SlotInfo currentSlot = _slotContainer.CurrentSlot;
			if (currentSlot != null)
			{
				UIUtility.SetLabelText(_textMaterialName, currentSlot.TextName);
				UIUtility.SetLabelText(_textMaterialCount, currentSlot.TextCount);
				UIUtility.SetLabelText(_textTagLevel, T._("{0:lv:} 이상", currentSlot.TagLevel));
				RefreshTagsAndMaterials(currentSlot.TextRequiredTags, currentSlot.TextRequiredMaterials);
			}
		}
	}

	private void RefreshTagsAndMaterials(string tags, string materials)
	{
		if (tags != string.Empty && materials != string.Empty)
		{
			UIUtility.SetLabelText(_textRequiredTags, tags);
			UIUtility.SetLabelText(_textRequiredMaterials, materials);
			((Component)_labelMaterial).gameObject.SetActive(true);
			((Component)_textRequiredMaterials).gameObject.SetActive(true);
		}
		else
		{
			UIUtility.SetLabelText(_textRequiredTags, (!(tags != string.Empty)) ? materials : tags);
			((Component)_labelMaterial).gameObject.SetActive(false);
			((Component)_textRequiredMaterials).gameObject.SetActive(false);
		}
	}

	private void RefreshItemList()
	{
		_refreshingItemList = true;
		SlotInfo currentSlot = _slotContainer.CurrentSlot;
		_warningDictionary.Clear();
		_itemList.ClearSelectItem(sendEvent: false);
		_itemList.SetItemList(Util.Filtering(GameSystem<InventorySystem>.Instance().PlayerItemList, (ItemData itemData) => currentSlot.IsSuitableItem(itemData, ignoreLevel: true)));
		_itemList.EquipmentsSelectable = currentSlot is RecipeToolInfo;
		if (currentSlot != null)
		{
			_itemList.SelectableCount = currentSlot.MaxCount - currentSlot.PreviouslyAssignedItemsCount;
			IList<ItemData> selectedItems = currentSlot.SelectedItems;
			for (int i = 0; i < selectedItems.Count; i++)
			{
				_itemList.SelectItem(selectedItems[i]);
			}
			CheckLockedItems(_itemList.Items, delegate(ulong id)
			{
				_warningDictionary[id] = MaterialInfoWidget.WarningType.Locked;
			});
			DisableInsufficientLevelItems(currentSlot, _itemList.Items, delegate(ulong id)
			{
				_warningDictionary[id] = MaterialInfoWidget.WarningType.InsufficientTagLevel;
			});
			_slotContainer.DisableAlreadySelectedItemIconsByOtherSlots(_itemList.Items, delegate(ulong id)
			{
				_warningDictionary[id] = MaterialInfoWidget.WarningType.SelectedByOtherSlot;
			});
			DisableUnmodifiableItemsIfModifyBaseSlot(currentSlot, _itemList.Items, delegate(ulong id)
			{
				_warningDictionary[id] = MaterialInfoWidget.WarningType.Unmodifiable;
			});
			AddAndDisablePreviouslyAssignedItems(delegate(ulong id)
			{
				_warningDictionary[id] = MaterialInfoWidget.WarningType.PreviouslyAssigned;
			});
		}
		_itemList.ResetPosition();
		_refreshingItemList = false;
	}

	private void CheckLockedItems(List<ItemIcon2> itemIcons, Action<ulong> enumerationDisabledIds)
	{
		for (int i = 0; i < itemIcons.Count; i++)
		{
			ItemIcon2 itemIcon = itemIcons[i];
			if (itemIcon.Item.Like)
			{
				enumerationDisabledIds(itemIcon.Item.Id);
			}
		}
	}

	private void DisableInsufficientLevelItems(SlotInfo currentSlot, List<ItemIcon2> itemIcons, Action<ulong> enumerationDisabledIds)
	{
		for (int i = 0; i < itemIcons.Count; i++)
		{
			ItemIcon2 itemIcon = itemIcons[i];
			if (!currentSlot.IsSuitableItem(itemIcon.Item))
			{
				itemIcon.IconMode = ItemIcon2.Mode.DisableButSelectable;
				itemIcon.LevelWarning = true;
				enumerationDisabledIds(itemIcon.Item.Id);
			}
		}
	}

	private void DisableUnmodifiableItemsIfModifyBaseSlot(SlotInfo currentSlot, List<ItemIcon2> itemIcons, Action<ulong> enumerationDisabledIds)
	{
		if (!(currentSlot is CraftSlotInfo { IsModifyBase: not false }))
		{
			return;
		}
		for (int i = 0; i < itemIcons.Count; i++)
		{
			ItemIcon2 itemIcon = itemIcons[i];
			if (itemIcon.Item.ModifiableCount < 1)
			{
				itemIcon.IconMode = ItemIcon2.Mode.DisableButSelectable;
				enumerationDisabledIds(itemIcon.Item.Id);
			}
		}
	}

	private void AddAndDisablePreviouslyAssignedItems(Action<ulong> enumerationDisabledIds)
	{
		IList<ItemData> previouslyAssignedItems = _slotContainer.CurrentSlot.PreviouslyAssignedItems;
		for (int i = 0; i < previouslyAssignedItems.Count; i++)
		{
			ItemIcon2 itemIcon = _itemList.AddFirst(previouslyAssignedItems[i]);
			itemIcon.Selected = true;
			itemIcon.IconMode = ItemIcon2.Mode.DisableWithSelectionMark;
			enumerationDisabledIds(itemIcon.Item.Id);
		}
	}

	private void RefreshMaterialInfo()
	{
		//IL_0069: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		ItemIcon2 lastClickedItem = _itemList.LastClickedItem;
		MaterialInfoWidget.WarningType warningType = (((Object)(object)lastClickedItem != (Object)null && _warningDictionary.ContainsKey(lastClickedItem.Item.Id)) ? _warningDictionary[lastClickedItem.Item.Id] : MaterialInfoWidget.WarningType.None);
		_textTagLevel.color = ((warningType != MaterialInfoWidget.WarningType.InsufficientTagLevel) ? _colorTagLevelNormal : _colorTagLevelWarning);
		_materialInfoWidget.SetMaterial(lastClickedItem, warningType);
	}

	private static MaterialInfoWidget CreateMaterialInfoWidget(GameObject container, MaterialInfoWidget prefab)
	{
		GameObject val = container.AddChild(((Component)prefab).gameObject);
		UIWidget component = val.GetComponent<UIWidget>();
		component.SetAnchor(container, 0, 0, 0, 0);
		return val.GetComponent<MaterialInfoWidget>();
	}

	private void OnUpdateSelectItem()
	{
		if (!_refreshingItemList)
		{
			SlotInfo currentSlot = _slotContainer.CurrentSlot;
			List<ItemIcon2> selectedItemList = _itemList.SelectedItemList;
			if (currentSlot != null)
			{
				currentSlot.SetSelectedItems(selectedItemList);
				UIUtility.SetLabelText(_textMaterialCount, currentSlot.TextCount);
				RefreshMaterialInfo();
			}
			if (this.ItemSelectionUpdated != null)
			{
				this.ItemSelectionUpdated();
			}
		}
	}
}
