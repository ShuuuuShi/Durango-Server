using System;
using System.Collections.Generic;
using Durango.Logic.InputSystem;
using Durango.Logic.Item;
using Durango.Render.Camera;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using Shared.Item;
using UnityEngine;

namespace Durango.UI;

public class EquipWidget_PC : EquipWidgetBase
{
	[SerializeField]
	private GameObject _characterAbilityWidget;

	[SerializeField]
	private GameObject _abilityList;

	[SerializeField]
	private GameObject _detailShowButton;

	[SerializeField]
	private ItemInfoContainer _itemInfo;

	private int _selectedTab;

	private List<Action> _tabClickEvents = new List<Action>();

	private StringSelector _stringSelector;

	protected override void Awake()
	{
		base.Awake();
		GameSystem<InputSystem>.Instance().On(InputCommand.PrevTab, OnInputTabShortcut);
		GameSystem<InputSystem>.Instance().On(InputCommand.NextTab, OnInputTabShortcut);
		_tabClickEvents.Add(base.TabForEquipPreset_Clicked);
		_tabClickEvents.Add(base.TabForAvatarEquip_Clicked);
		_itemInfo.Hide();
		_equipButton.gameObject.SetActive(value: false);
	}

	public override void Init()
	{
		base.Init();
		if (_itemList != null)
		{
			ItemList itemList = _itemList;
			itemList.OnItemIconRightClick = (Action)Delegate.Combine(itemList.OnItemIconRightClick, new Action(OnItemIconRightClick));
		}
	}

	protected override void RefreshEquipSlotContainer()
	{
		base.RefreshEquipSlotContainer();
		if (base.SelectedEquipPreset != EquipSlotType.Invalid)
		{
			bool flag = base.SelectedEquipPreset == EquipSlotType.Avatar;
			bool flag2 = GameSystem<EquipSystem>.Instance().IsLockedPreset(base.SelectedEquipPreset);
			_characterAbilityWidget.SetActive(flag || !flag2);
			_abilityList.SetActive(!flag);
			_detailShowButton.SetActive(!flag);
		}
		else
		{
			_abilityList.SetActive(value: true);
			_detailShowButton.SetActive(value: true);
		}
	}

	protected override void SetTitle(string title)
	{
		SelectableWidget component = _titleSelector.GetComponent<SelectableWidget>();
		if (!(component == null))
		{
			if (string.IsNullOrEmpty(title))
			{
				_titleLabel.text = T._("칭호 없음");
				component.Selected = false;
			}
			else
			{
				_titleLabel.text = title;
				component.Selected = true;
			}
		}
	}

	protected override void SelectEquipPreset(EquipSlotType presetType)
	{
		base.SelectEquipPreset(presetType);
		DeselectAllSlot();
		_itemInfo.Hide();
		_equipButton.gameObject.SetActive(value: false);
	}

	private void DeselectAllSlot()
	{
		_equipSlotWidget.DeselectAllSlot();
		RefreshItemList();
		base.LastSelected = null;
		RefreshEquipButton();
	}

	protected override void EquipSlotWidget_SlotClicked(EquipSystem.Slot slot)
	{
		base.EquipSlotWidget_SlotClicked(slot);
		_itemInfo.Hide();
		_equipButton.gameObject.SetActive(value: false);
		SelectEquipedItem();
	}

	private void SelectEquipedItem()
	{
		foreach (ItemData item in _itemList)
		{
			if (item.IsEquipments)
			{
				_itemList.SelectItem(item, sendEvent: true, scrollTo: true);
				break;
			}
		}
	}

	private void OnInputTabShortcut(InputCommandMessage message)
	{
		if (message.CurrentTrigger == Trigger.Up)
		{
			_selectedTab += ((message.Command == InputCommand.NextTab) ? 1 : (-1));
			if (_selectedTab >= _tabClickEvents.Count)
			{
				_selectedTab = 0;
			}
			if (_selectedTab < 0)
			{
				_selectedTab = _tabClickEvents.Count - 1;
			}
			if (_tabClickEvents[_selectedTab] != null)
			{
				_tabClickEvents[_selectedTab]();
			}
		}
	}

	protected override void ItemList_OnUpdateSelectItem()
	{
		base.LastSelected = _itemList.LastSelectedItem;
		RefreshEquipButton();
		if (base.LastSelected == null)
		{
			_itemInfo.Hide();
			_equipButton.gameObject.SetActive(value: false);
		}
		else
		{
			_itemInfo.Show(base.LastSelected);
			_equipButton.gameObject.SetActive(value: true);
		}
	}

	protected override void OnItemIconRightClick()
	{
		if (base.LastSelected != null)
		{
			List<string> list = new List<string>();
			list.Add((!base.LastSelected.IsEquipments) ? T._("장착") : T._("해제"));
			StringSelector stringSelector = UIManager.Popup.Tooltip<StringSelector>();
			stringSelector.Set(list, OnSelectContextAction);
			stringSelector.AddOnFinished(OnHideContextAction);
			stringSelector.MinWidth = 170;
			stringSelector.MaxWidth = 170;
			stringSelector.Show();
			Vector3 pos = MainCamera.ScreenPosToNGUIPos(new Vector3(UICamera.currentTouch.pos.x, UICamera.currentTouch.pos.y, 0f)) + Vector3.down * 5f;
			stringSelector.Widget.SetPosition(pos, 0f, 1f);
			stringSelector.IntoSafeArea();
			_stringSelector = stringSelector;
		}
	}

	private void OnSelectContextAction(int index)
	{
		ToggleEquipLastSelectedItem();
	}

	private void OnHideContextAction()
	{
		if (!(_stringSelector == null))
		{
			_stringSelector.Hide();
			_stringSelector = null;
		}
	}
}
