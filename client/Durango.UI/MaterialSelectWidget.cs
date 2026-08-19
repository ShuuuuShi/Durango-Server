using System;
using System.Collections.Generic;
using Crafting;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class MaterialSelectWidget : MonoBehaviour, IUIInitializable
{
	public enum WarningType
	{
		None,
		[T.EnumName("[icon=icon_make_alert] 아이템의 레벨이 낮습니다.")]
		InsufficientTagLevel,
		[T.EnumName("[icon=icon_make_alert] 가공 횟수가 부족합니다.")]
		Unmodifiable,
		[T.EnumName("[icon=icon_make_alert] 다른 슬롯에서 이미 선택되었습니다.")]
		SelectedByOtherSlot,
		[T.EnumName("[icon=icon_make_alert] 이미 건설에 투입된 아이템입니다.")]
		PreviouslyAssigned,
		[T.EnumName("[icon=icon_make_alert] 보호 중인 아이템입니다.")]
		Locked,
		[T.EnumName("[icon=icon_make_alert] 이미 개조된 아이템입니다.")]
		AlreadyReformed
	}

	[SerializeField]
	private UIWidget _titleWidget;

	[SerializeField]
	private UILabel _textMaterialName;

	[SerializeField]
	private UILabel _textMaterialCount;

	[SerializeField]
	private UIWidget _helpLinkSprite;

	[SerializeField]
	private UILabel _textTagLevel;

	[SerializeField]
	private UILabel _textRequiredTags;

	[SerializeField]
	private UILabel _labelMaterial;

	[SerializeField]
	private UILabel _textRequiredMaterials;

	[SerializeField]
	private Selectable _marketSearchButton;

	[SerializeField]
	private NestedPrefabLinker _itemListLinker;

	[SerializeField]
	private ItemInfoContainer _itemInfo;

	[SerializeField]
	private Color _colorTagLevelNormal;

	[SerializeField]
	private Color _colorTagLevelWarning;

	private SlotContainer _slotContainer;

	private ItemList _itemList;

	private readonly Dictionary<string, WarningType> _warningDictionary = new Dictionary<string, WarningType>();

	private readonly List<ItemData> _sortedList = new List<ItemData>();

	private bool _refreshingItemList;

	private UIWidget[][] _itemWidgets;

	private float[][] _itemMargins;

	public event Action ItemSelectionUpdated;

	void IUIInitializable.Init()
	{
		_itemList = _itemListLinker.Object.GetComponent<ItemList>();
		_itemList.FixedIconSize = true;
		_itemList.MultiIconMode = ItemIconWidget.MultiIconMode.Index;
		_itemWidgets = new UIWidget[2][]
		{
			new UIWidget[5] { _textMaterialName, _textMaterialCount, _helpLinkSprite, _labelMaterial, _textRequiredMaterials },
			new UIWidget[2] { _textTagLevel, _textRequiredTags }
		};
		_itemMargins = new float[_itemWidgets.Length][];
		for (int i = 0; i < _itemWidgets.Length; i++)
		{
			_itemMargins[i] = new float[_itemWidgets[i].Length - 1];
			for (int j = 0; j < _itemMargins[i].Length; j++)
			{
				_itemMargins[i][j] = _itemWidgets[i][j + 1].GetPosition(0f, 0f).x - _itemWidgets[i][j].GetPosition(1f, 0f).x;
			}
		}
		_itemList.OnUpdateSelectItem = OnUpdateSelectItem;
		_marketSearchButton.gameObject.SetActive(GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Market));
		Selectable marketSearchButton = _marketSearchButton;
		marketSearchButton.Clicked = (Action)Delegate.Combine(marketSearchButton.Clicked, (Action)delegate
		{
			SlotInfo currentSlot = _slotContainer.CurrentSlot;
			if (currentSlot != null)
			{
				MarketGroup marketGroup = UIManager.FindScript<MarketGroup>();
				marketGroup.OpenAndSearch(currentSlot.RequiredTags, currentSlot.RequiredMaterials, currentSlot.RequiredLevel);
			}
		});
		_labelMaterial.text = T._("재질");
		UIEventListener.Get(_helpLinkSprite.gameObject).onClick = OnClickHelpLink;
	}

	private void OnEnable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += RefreshItemList;
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= RefreshItemList;
	}

	public void Set(SlotContainer slotContainer)
	{
		_slotContainer = slotContainer;
	}

	public void Refresh()
	{
		RefreshUpperBar();
		RefreshItemList();
		RefreshMaterialInfo();
	}

	public void ResetpositionItemList()
	{
		_itemList.ResetPosition();
	}

	public ItemIconWidget GetFirstSelectableEnabledItemOrNull()
	{
		return _itemList.GetFirstSelectableEnabledItemOrNull();
	}

	private void OnClickHelpLink(GameObject obj)
	{
		SlotInfo currentSlot = _slotContainer.CurrentSlot;
		SlotInfoPopup slotInfoPopup = UIManager.Popup.Tooltip<SlotInfoPopup>();
		slotInfoPopup.Set(currentSlot.Name, currentSlot.RequiredLevel, currentSlot.RequiredTags, currentSlot.RequiredMaterials, currentSlot.SlotSourceInfo);
		slotInfoPopup.Show();
	}

	private void RefreshUpperBar()
	{
		if (_slotContainer == null)
		{
			return;
		}
		SlotInfo currentSlot = _slotContainer.CurrentSlot;
		if (currentSlot == null)
		{
			return;
		}
		_textMaterialName.text = currentSlot.Name;
		_textMaterialCount.text = $"{currentSlot.CurrentCount} / {currentSlot.TotalCount}";
		if (currentSlot.RequiredLevel > 1)
		{
			_textTagLevel.gameObject.SetActive(value: true);
			_textTagLevel.text = T._("{0:lv:} 이상", currentSlot.RequiredLevel);
		}
		else
		{
			_textTagLevel.gameObject.SetActive(value: false);
		}
		RefreshTagsAndMaterials(Util.LocalizedTagRequiredMsg(currentSlot.RequiredTags, showLevel: false), Util.LocalizedTagRequiredMsg(currentSlot.RequiredMaterials, showLevel: false));
		for (int i = 0; i < _itemWidgets.Length; i++)
		{
			UIWidget uIWidget = null;
			for (int j = 0; j < _itemWidgets[i].Length; j++)
			{
				UIWidget uIWidget2 = _itemWidgets[i][j];
				if (uIWidget2.gameObject.activeSelf)
				{
					float x = ((!(uIWidget == null)) ? (uIWidget.GetPosition(1f, 0f).x + _itemMargins[i][j - 1]) : _itemWidgets[i][0].GetPosition(0f, 0f).x);
					Vector3 pos = new Vector3(x, uIWidget2.transform.localPosition.y);
					uIWidget2.SetPosition(pos, 0f, uIWidget2.pivotOffset.y);
					uIWidget = uIWidget2;
				}
			}
		}
		UIUtility.UpdateAnchors(_titleWidget.transform);
	}

	private void RefreshTagsAndMaterials(string tags, string materials)
	{
		bool flag = string.IsNullOrEmpty(tags);
		bool flag2 = string.IsNullOrEmpty(materials);
		if (!flag && !flag2)
		{
			_textRequiredTags.text = tags;
			_textRequiredMaterials.text = materials;
			_labelMaterial.gameObject.SetActive(value: true);
			_textRequiredMaterials.gameObject.SetActive(value: true);
		}
		else
		{
			_textRequiredTags.text = ((!flag) ? tags : materials);
			_labelMaterial.gameObject.SetActive(value: false);
			_textRequiredMaterials.gameObject.SetActive(value: false);
		}
	}

	private void RefreshItemList()
	{
		_refreshingItemList = true;
		SlotInfo currentSlot = _slotContainer.CurrentSlot;
		_warningDictionary.Clear();
		_itemList.DeselectAllItems(sendEvent: false);
		IList<ItemData> previouslyAssignedItems = _slotContainer.CurrentSlot.PreviouslyAssignedItems;
		_sortedList.Clear();
		List<ItemData> playerItemList = GameSystem<InventorySystem>.Instance().PlayerItemList;
		for (int i = 0; i < playerItemList.Count; i++)
		{
			ItemData itemData = playerItemList[i];
			if (currentSlot.IsSuitableItem(itemData, ignoreSubReason: true))
			{
				_sortedList.Add(itemData);
			}
		}
		_sortedList.Sort(_slotContainer.ItemPriorityComparison);
		if (previouslyAssignedItems == null)
		{
			_itemList.SetItemList(_sortedList);
		}
		else
		{
			_itemList.SetItemList(new ItemList.SetStruct[2]
			{
				new ItemList.SetStruct
				{
					List = _sortedList
				},
				new ItemList.SetStruct
				{
					List = previouslyAssignedItems,
					OnInit = delegate(ItemIconWidget icon)
					{
						icon.IconMode = ItemIconWidget.Mode.DisabledWithSelectionMark;
						_warningDictionary[icon.Item.Id] = WarningType.PreviouslyAssigned;
					}
				}
			});
		}
		_itemList.EquipmentsSelectable = currentSlot is RecipeToolInfo;
		if (currentSlot != null)
		{
			int totalCount = currentSlot.TotalCount;
			_itemList.SelectableCount = totalCount - currentSlot.PreviouslyAssignedItemsCount;
			IList<ItemData> selectedItems = currentSlot.SelectedItems;
			for (int j = 0; j < selectedItems.Count; j++)
			{
				_itemList.SelectItem(selectedItems[j], sendEvent: false, scrollTo: false);
			}
			if (currentSlot.SelectedItems.Count != _itemList.SelectedList.Count)
			{
				currentSlot.SetSelectedItems(_itemList.SelectedList);
			}
			CheckLockedItems();
			DisableAlreadySelectedItemIconsByOtherSlots(currentSlot);
			DisableInsufficientLevelItems(currentSlot);
			DisableBaseItemsIfBaseSlot(currentSlot);
			OnUpdateSelectItem();
		}
		_itemList.Reposition();
		_refreshingItemList = false;
	}

	private void CheckLockedItems()
	{
		for (int i = 0; i < _itemList.Count; i++)
		{
			ItemData itemData = _itemList[i];
			if (itemData.Locked)
			{
				_warningDictionary[itemData.Id] = WarningType.Locked;
			}
		}
	}

	private void DisableAlreadySelectedItemIconsByOtherSlots(SlotInfo currentSlot)
	{
		HashSet<string> selected = _slotContainer.GatherOtherSlotsSelectedItemIds(currentSlot);
		_itemList.ForEachIcon(delegate(ItemIconWidget icon)
		{
			string id = icon.Item.Id;
			if (selected.Contains(id))
			{
				icon.IconMode = ItemIconWidget.Mode.DisabledWithSelectionMark;
				_warningDictionary[id] = WarningType.SelectedByOtherSlot;
			}
		});
		if (currentSlot is TechSupportBaseSlotInfo)
		{
			_itemList.ForEachIcon(delegate(ItemIconWidget icon)
			{
				icon.IconMode = ItemIconWidget.Mode.DisabledWithSelectionMark;
			});
		}
	}

	private void DisableInsufficientLevelItems(SlotInfo currentSlot)
	{
		_itemList.ForEachIcon(delegate(ItemIconWidget icon)
		{
			ItemData item = icon.Item;
			if (!currentSlot.IsSuitableItem(item))
			{
				icon.IconMode = ItemIconWidget.Mode.Disabled;
				icon.LevelWarning = true;
				_warningDictionary[item.Id] = WarningType.InsufficientTagLevel;
			}
		});
	}

	private void DisableBaseItemsIfBaseSlot(SlotInfo currentSlot)
	{
		if (!(currentSlot is CraftSlotInfo craftSlotInfo))
		{
			return;
		}
		if (craftSlotInfo is TechSupportBaseSlotInfo)
		{
			_itemList.ForEachIcon(delegate(ItemIconWidget icon)
			{
				icon.IconMode = ItemIconWidget.Mode.DisabledWithSelectionMark;
			});
			return;
		}
		switch (craftSlotInfo.SlotType)
		{
		case RecipeSlot.Type.ModifyBase:
			_itemList.ForEachIcon(delegate(ItemIconWidget icon)
			{
				ItemData item = icon.Item;
				if (item.ModifiableCount < 1)
				{
					icon.IconMode = ItemIconWidget.Mode.Disabled;
					_warningDictionary[item.Id] = WarningType.Unmodifiable;
				}
			});
			break;
		case RecipeSlot.Type.ReformBase:
			_itemList.ForEachIcon(delegate(ItemIconWidget icon)
			{
				if (!TechSupportTarget.HasEmptyReformSlot(icon.Item))
				{
					icon.IconMode = ItemIconWidget.Mode.Disabled;
					_warningDictionary[icon.Item.Id] = WarningType.AlreadyReformed;
				}
			});
			break;
		}
	}

	private void RefreshMaterialInfo()
	{
		ItemData lastClickedItem = _itemList.LastClickedItem;
		WarningType warningType = ((lastClickedItem != null && _warningDictionary.ContainsKey(lastClickedItem.Id)) ? _warningDictionary[lastClickedItem.Id] : WarningType.None);
		_textTagLevel.color = ((warningType != WarningType.InsufficientTagLevel) ? _colorTagLevelNormal : _colorTagLevelWarning);
		_itemInfo.Show(lastClickedItem, (warningType == WarningType.None) ? null : warningType.GetName());
	}

	private void OnUpdateSelectItem()
	{
		if (_refreshingItemList)
		{
			return;
		}
		SlotInfo currentSlot = _slotContainer.CurrentSlot;
		List<ItemData> selectedList = _itemList.SelectedList;
		if (currentSlot != null)
		{
			currentSlot.SetSelectedItems(selectedList);
			if ((bool)_textMaterialCount)
			{
				_textMaterialCount.text = $"{currentSlot.CurrentCount} / {currentSlot.TotalCount}";
			}
			RefreshMaterialInfo();
		}
		if (this.ItemSelectionUpdated != null)
		{
			this.ItemSelectionUpdated();
		}
	}
}
