using System;
using System.Collections.Generic;
using ItemSystem;
using L10N;
using UnityEngine;

public class EquipGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private EquipSlotsWidget _equipSlotWidget;

	[SerializeField]
	private DefaultSelectableButton _equipButton;

	[SerializeField]
	private UIWidget _inventoryContianer;

	[SerializeField]
	private ItemList _itemList;

	[SerializeField]
	private GameObject _equipSlots;

	[SerializeField]
	private GameObject _itemCountZero;

	[SerializeField]
	private EquipStatWidget _equipStatWidget;

	[SerializeField]
	private EquipItemActionWidget _equipItemActionWidget;

	private ItemIcon2 _lastSelected;

	private bool _isEquipChange;

	private EquipSystem.PlayerEquipInfo _playerEquipInfo = new EquipSystem.PlayerEquipInfo();

	private ItemIcon2 LastSelected
	{
		get
		{
			return _lastSelected;
		}
		set
		{
			_lastSelected = value;
			EquipSystem.Slot selectedSlot = _equipSlotWidget.SelectedSlot;
			if (selectedSlot == EquipSystem.Slot.Invalid)
			{
				_equipButton.Disable = true;
				_equipButton.Text = T._("장착");
				return;
			}
			_equipButton.Disable = false;
			if ((Object)(object)_lastSelected == (Object)null)
			{
				EquipSlot slot = _equipSlotWidget.GetSlot(selectedSlot);
				if (slot.Item == null)
				{
					_equipButton.Disable = true;
					_equipButton.Text = T._("장착");
				}
				else
				{
					_equipButton.Disable = slot.Disable;
					_equipButton.Text = T._("해제");
				}
			}
			else if (_lastSelected.Item.IsEquipments)
			{
				_equipButton.Text = T._("해제");
			}
			else
			{
				_equipButton.Text = T._("장착");
			}
		}
	}

	public event Action<EquipSystem.Slot> EquipSlotClicked;

	private void Awake()
	{
		SetOpenCloseSound("Sound/Effect/UI/UI_Menu_Equip_Open_01.wav", "Sound/Effect/UI/UI_Menu_Equip_Close_01.wav");
		OnClose();
	}

	private void Start()
	{
		_equipSlotWidget.SlotClicked += OnClickEquipSlot;
		_equipButton.Clicked = OnEquip;
		_titleWidget.OnClose += base.ForceClose;
		_itemList.SelectableCount = 1;
		_itemList.FixedIconSize = true;
		_itemList.EquipmentsSelectable = true;
		_itemList.OnUpdateSelectItem = OnUpdateSelectItem;
		base.OnOpenSucceed += OpenSucceed;
		base.OnCloseSucceed += CloseSucceed;
	}

	private void OnEnable()
	{
		GameSystem<EquipSystem>.Instance().OnUpdateEquipments += OnUpdateEquipments;
		GameSystem<StatisticsSystem>.Instance().AbilitiesUpdated += OnUpdateEquipments;
	}

	private void OnDisable()
	{
		GameSystem<EquipSystem>.Instance().OnUpdateEquipments -= OnUpdateEquipments;
		GameSystem<StatisticsSystem>.Instance().AbilitiesUpdated -= OnUpdateEquipments;
	}

	private void OpenSucceed()
	{
		((Component)_inventoryContianer).gameObject.SetActive(false);
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += UpdateItemsFilteringBySlot;
		GameSystem<InventorySystem>.Instance().PlayerInventory.UpdateIfNeeded();
		LastSelected = null;
		_isEquipChange = false;
		OnUpdateEquipments();
	}

	private void CloseSucceed()
	{
		_equipSlotWidget.SelectSlot(EquipSystem.Slot.Invalid);
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= UpdateItemsFilteringBySlot;
		if (_isEquipChange)
		{
			_isEquipChange = false;
			KSingleton<PlayerController>.Instance().Motion("Avatar_Dress");
		}
	}

	private void UpdateEquipItemInfo()
	{
		_playerEquipInfo = GameSystem<EquipSystem>.Instance().GetEquipInfo();
		_equipStatWidget.SetEquipInfo(_playerEquipInfo);
		_equipItemActionWidget.SetAction(GameSystem<CombatSystem>.Instance().CurrentActiveActions);
	}

	private void OnUpdateEquipments()
	{
		if (base.IsOpen)
		{
			_equipSlotWidget.SetSlots(GameSystem<EquipSystem>.Instance().EquipItems);
			UpdateEquipItemInfo();
		}
	}

	private void OnClickEquipSlot(EquipSystem.Slot key)
	{
		if (key != EquipSystem.Slot.Invalid)
		{
			_equipSlotWidget.SelectSlot(key);
			LastSelected = null;
			UpdateItemsFilteringBySlot();
			if (this.EquipSlotClicked != null)
			{
				this.EquipSlotClicked(key);
			}
		}
	}

	public void UpdateItemsFilteringBySlot()
	{
		EquipSystem.Slot selectedSlot = _equipSlotWidget.SelectedSlot;
		if (selectedSlot == EquipSystem.Slot.Invalid)
		{
			return;
		}
		_itemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList);
		_itemList.ClearSelectItem(sendEvent: false);
		_itemList.ResetFilters();
		switch (selectedSlot)
		{
		case EquipSystem.Slot.Main:
		{
			KeyValuePair<string, string>[] filter2 = new KeyValuePair<string, string>[2];
			ref KeyValuePair<string, string> reference3 = ref filter2[0];
			reference3 = new KeyValuePair<string, string>("slot", "main");
			ref KeyValuePair<string, string> reference4 = ref filter2[1];
			reference4 = new KeyValuePair<string, string>("slot", "both");
			_itemList.Filter((ItemData data) => data.HasAttribute(filter2));
			break;
		}
		case EquipSystem.Slot.Body:
		{
			KeyValuePair<string, string>[] filter = new KeyValuePair<string, string>[2];
			ref KeyValuePair<string, string> reference = ref filter[0];
			reference = new KeyValuePair<string, string>("slot", "body");
			ref KeyValuePair<string, string> reference2 = ref filter[1];
			reference2 = new KeyValuePair<string, string>("slot", "hoody");
			_itemList.Filter((ItemData data) => data.HasAttribute(filter));
			break;
		}
		default:
		{
			string slot = selectedSlot.ToString().ToLower();
			_itemList.Filter((ItemData data) => data.HasAttribute("slot", slot));
			break;
		}
		}
		int usableCount = _itemList.UsableCount;
		_itemCountZero.SetActive(usableCount == 0);
		((Component)_inventoryContianer).gameObject.SetActive(usableCount > 0);
		_itemList.Reposition(reset: false, useTween: false);
	}

	private void OnEquip()
	{
		if ((Object)(object)Selectable.Current != (Object)null && Selectable.Current.Disable)
		{
			return;
		}
		EquipSystem.Slot selectedSlot = _equipSlotWidget.SelectedSlot;
		if (selectedSlot != EquipSystem.Slot.Invalid)
		{
			if ((Object)(object)LastSelected == (Object)null || LastSelected.Item.IsEquipments)
			{
				GameSystem<EquipSystem>.Instance().EquipItem(selectedSlot, null);
				_isEquipChange = true;
				return;
			}
			GameSystem<EquipSystem>.Instance().EquipItem(selectedSlot, LastSelected.Item);
			_isEquipChange = true;
			LastSelected.Selected = false;
			LastSelected = null;
		}
	}

	private void OnUpdateSelectItem()
	{
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008a: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a9: Unknown result type (might be due to invalid IL or missing references)
		ItemIcon2 lastClickedItem = _itemList.LastClickedItem;
		if ((Object)(object)lastClickedItem == (Object)null && (Object)(object)LastSelected == (Object)null)
		{
			return;
		}
		if ((Object)(object)lastClickedItem != (Object)null)
		{
			if (lastClickedItem.IconMode == ItemIcon2.Mode.Enable)
			{
				LastSelected = lastClickedItem;
				ItemInfoPopup itemInfoPopup = UIManager.Popup.Tooltip<ItemInfoPopup>();
				itemInfoPopup.Sign = -1;
				itemInfoPopup.Set(lastClickedItem.Item);
				itemInfoPopup.Show(_inventoryContianer, Vector2.up * (1f - _inventoryContianer.pivotOffset.y) * (float)_inventoryContianer.height, 10f);
				itemInfoPopup.HideArrow();
			}
		}
		else
		{
			OnEquip();
		}
	}
}
