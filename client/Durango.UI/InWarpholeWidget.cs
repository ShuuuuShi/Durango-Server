using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI;

public class InWarpholeWidget : AnimationWidget
{
	[SerializeField]
	private NestedPrefabLinker _itemListLinker;

	[SerializeField]
	private UILabel _remainSizeLabel;

	[SerializeField]
	private SelectableButton _submitButton;

	[SerializeField]
	private ItemInfoContainer _itemInfo;

	private ItemList _itemList;

	private CargoReceiver _receiver;

	private int _costPerSize;

	private CargoWarpholeGroup _parent;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_parent = GetComponentInParent<CargoWarpholeGroup>();
			_itemList = _itemListLinker.Object.GetComponent<ItemList>();
			ItemList itemList = _itemList;
			itemList.OnChangeItemList = (Action)Delegate.Combine(itemList.OnChangeItemList, new Action(OnUpdateInventorySelectedItems));
			ItemList itemList2 = _itemList;
			itemList2.OnUpdateSelectItem = (Action)Delegate.Combine(itemList2.OnUpdateSelectItem, new Action(OnUpdateInventorySelectedItems));
			_submitButton.Clicked = OnSubmit;
			_itemList.SelectableCount = -1;
			_itemList.MultiIconMode = ItemIconWidget.MultiIconMode.Index;
			SetAlpha(0f, useTween: false);
		}
	}

	private void OnEnable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated += OnUpdatePlayerInventory;
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().PlayerInventoryUpdated -= OnUpdatePlayerInventory;
		SetAlpha(0f, useTween: false);
	}

	public void Open(CargoReceiver receiver, int costPerSize)
	{
		Init();
		_receiver = receiver;
		_costPerSize = costPerSize;
		ResetData();
		base.gameObject.SetActive(value: true);
		base.Alpha = 1f;
	}

	public void Close(bool instant)
	{
		SetAlpha(0f, !instant);
	}

	private void ResetData()
	{
		_itemList.DeselectAllItems(sendEvent: false);
		Refresh();
	}

	private void Refresh()
	{
		_itemList.SetItemList(GameSystem<InventorySystem>.Instance().PlayerItemList);
	}

	private void OnUpdateInventorySelectedItems()
	{
		int num = _receiver.MaxSize - _receiver.UsingSize;
		if (num == 0)
		{
			UIManager.SystemMsg(T._("화물 워프홀이 꽉 차서 전송이 불가능합니다"));
		}
		List<ItemData> selectedList = _itemList.SelectedList;
		int num2 = 0;
		for (int i = 0; i < selectedList.Count; i++)
		{
			num2 += selectedList[i].Size;
		}
		while (num2 > num)
		{
			if (_itemList.SelectedList.Count == 0)
			{
				num2 = 0;
				break;
			}
			ItemData itemData = _itemList.SelectedList[0];
			num2 -= itemData.Size;
			_itemList.SelectItem(itemData, sendEvent: false, scrollTo: false);
		}
		long num3 = num2 * _costPerSize;
		_remainSizeLabel.text = $"<em>{num2}</em>[FFFFFF7F]/[-]{num}";
		_submitButton.Text = ((num3 <= 0) ? T._("전송") : Durango.Logic.Item.Inventory.ToCurrencyButtonText(T._("전송"), num3, Currency.TStone));
		_submitButton.Disabled = _itemList.SelectedList.Count == 0;
		ItemData itemData2 = _itemList.LastClickedItem;
		if (itemData2 == null)
		{
			itemData2 = _itemList.LastSelectedItem;
		}
		_itemInfo.Show(itemData2);
	}

	private void OnSubmit()
	{
		List<ItemData> list = _itemList.SelectedList;
		if (list.Count == 0)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i < list.Count; i++)
		{
			num += list[i].Size;
		}
		long cost = num * _costPerSize;
		string comment;
		if (list.Count == 1)
		{
			comment = list[0].SafeLevel switch
			{
				SafeLevel.Locked => T._("<em>잠금</em> 설정된 <em>{0}</em>{0:-을} 전송하시겠습니까?", list[0].Name), 
				SafeLevel.Protected => T._("<em>임무</em> 수행에 필요한 <em>{0}</em>{0:-을} 전송하시겠습니까?", list[0].Name), 
				_ => T._("<em>{0}</em>{0:-을} 전송하시겠습니까?", list[0].Name), 
			};
		}
		else
		{
			ItemData itemData = list.MaxBy((ItemData x) => x.SafeLevel);
			comment = (itemData?.SafeLevel ?? SafeLevel.None) switch
			{
				SafeLevel.Locked => T._("<em>잠금</em> 설정된 <em>{0}</em> 외 {1}개 물품을 전송하시겠습니까?", itemData.Name, list.Count - 1), 
				SafeLevel.Protected => T._("<em>임무</em> 수행에 필요한 <em>{0}</em> 외 {1}개 물품을 전송하시겠습니까?", itemData.Name, list.Count - 1), 
				_ => T._("<em>{0}</em> 외 {1}개 물품을 전송하시겠습니까?", list[0].Name, list.Count - 1), 
			};
		}
		UIManager.MessageBox.ShowPayConfirm(cost, Currency.TStone, comment, delegate(bool ok)
		{
			if (ok)
			{
				string[] itemIds = Util.ItemsToIds(list);
				_submitButton.ShowLoadingRing(show: true);
				CargoWarpholeSystem.SendCargo(_parent.Id, _parent.Tile, _receiver, itemIds, delegate(CargoReceiver msg)
				{
					_submitButton.ShowLoadingRing(show: false);
					_receiver = msg;
					Refresh();
				});
			}
		}, T._("예"));
	}

	private void OnUpdatePlayerInventory()
	{
		Refresh();
	}
}
