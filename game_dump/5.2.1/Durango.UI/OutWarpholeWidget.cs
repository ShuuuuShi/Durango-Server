using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class OutWarpholeWidget : AnimationWidget
{
	[SerializeField]
	private TransmissionQueueWidget _queueWidget;

	[SerializeField]
	private TransmissionCompletedWidget _completedWidget;

	[SerializeField]
	private TransmissionQueueDetailWidget _queueDetailWidget;

	[SerializeField]
	private ItemInfoContainer _itemInfoWidget;

	[SerializeField]
	private GameObject _buttonContainer;

	[SerializeField]
	private SelectableButton _actionButton;

	private readonly List<ReceivingItem> _receivingItems = new List<ReceivingItem>();

	private readonly List<ItemData> _receivedItems = new List<ItemData>();

	private CargoWarpholeGroup _parent;

	private int _capacity;

	private float _nextUpdateAt;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_parent = UIUtility.FindComponentInParent<CargoWarpholeGroup>(base.gameObject);
			_queueWidget.ItemSelected += OnSelectedQueueItem;
			_queueWidget.DetailButtonClicked += OnDetailQueueButtonClick;
			_completedWidget.SelectedListUpdated += OnUpdateSelectedCompletedItemList;
			_queueDetailWidget.ItemSelected += OnSelectedQueueItem;
			SelectableButton actionButton = _actionButton;
			actionButton.Clicked = (Action)Delegate.Combine(actionButton.Clicked, new Action(OnClickActionButton));
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
		_queueWidget.ResetData();
		_queueDetailWidget.ResetData();
		SetAlpha(0f, useTween: false);
	}

	private void Update()
	{
		if (_nextUpdateAt > 0f && _nextUpdateAt < Time.time)
		{
			Refresh();
		}
	}

	public void Open(ReceivedItems items)
	{
		Init();
		SetNormalPage(instant: true);
		base.gameObject.SetActive(value: true);
		base.Alpha = 1f;
		_capacity = items.MaxSize;
		_receivingItems.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(items.ReceivingItems); i < size; i++)
		{
			_receivingItems.Add(new ReceivingItem(items.ReceivingItems[i]));
		}
		_receivingItems.Sort(RecevingItemComparison);
		_receivedItems.Clear();
		int j = 0;
		for (int size2 = KUtility.GetSize(items._ReceivedItems); j < size2; j++)
		{
			_receivedItems.Add(new ItemData(items._ReceivedItems[j]));
		}
		Refresh();
	}

	public void Close(bool instant)
	{
		SetAlpha(0f, !instant);
		_nextUpdateAt = 0f;
	}

	public bool Back()
	{
		if (!_queueDetailWidget.gameObject.activeSelf || _queueDetailWidget.Alpha < 1f)
		{
			return true;
		}
		SetNormalPage(instant: false);
		return false;
	}

	private void SetNormalPage(bool instant)
	{
		_queueWidget.gameObject.SetActive(value: true);
		_queueWidget.SetAlpha(1f, !instant);
		_completedWidget.gameObject.SetActive(value: true);
		_completedWidget.SetAlpha(1f, !instant);
		_queueDetailWidget.SetAlpha(0f, !instant);
	}

	private void SetDetailPage(bool instant)
	{
		_queueDetailWidget.gameObject.SetActive(value: true);
		_queueDetailWidget.SetAlpha(1f, !instant);
		_queueWidget.SetAlpha(0f, !instant);
		_completedWidget.SetAlpha(0f, !instant);
	}

	private void Refresh()
	{
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		int num = _receivingItems.Count;
		for (int i = 0; i < _receivingItems.Count; i++)
		{
			if (_receivingItems[i].ReceivingAt > predictedServerTime)
			{
				num = i;
				break;
			}
		}
		for (int j = 0; j < num; j++)
		{
			ItemData item = _receivingItems[j].Item;
			if (item != null)
			{
				item.Unstable = false;
				_receivedItems.Add(item);
			}
		}
		_receivingItems.RemoveRange(0, num);
		int num2 = 0;
		for (int k = 0; k < _receivedItems.Count; k++)
		{
			num2 += _receivedItems[k].Size;
		}
		_completedWidget.Set(_receivedItems, _capacity);
		_queueWidget.Set(_receivingItems, _capacity - num2);
		_queueDetailWidget.Set(_receivingItems, _capacity - num2);
		if (_receivingItems.Count > 0)
		{
			double num3 = _receivingItems[0].ReceivingAt - predictedServerTime;
			_nextUpdateAt = Time.time + (float)num3;
		}
		else
		{
			_nextUpdateAt = 0f;
		}
		SelectedItemUpdated();
	}

	private void OnSelectedQueueItem(string id)
	{
		_completedWidget.List.DeselectAllItems(sendEvent: false);
		_queueWidget.SelectItemWidget(id);
		_queueDetailWidget.SelectItemWidget(id);
		SelectedItemUpdated();
	}

	private void OnUpdateSelectedCompletedItemList()
	{
		_queueWidget.SelectItemWidget(null);
		_queueDetailWidget.SelectItemWidget(null);
		SelectedItemUpdated();
	}

	private void SelectedItemUpdated()
	{
		ItemData item = null;
		if (_queueWidget.Selected.Item != null)
		{
			_buttonContainer.SetActive(value: true);
			ReceivingItem selected = _queueWidget.Selected;
			double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
			double num = Math.Max(predictedServerTime, selected.WarpStartsAt);
			long immediateReceivingCost = Singleton<CostsYaml>.Instance.Cargo.GetImmediateReceivingCost(selected.ReceivingAt - num);
			bool flag = predictedServerTime > selected.WarpStartsAt;
			if (flag && immediateReceivingCost > 0)
			{
				_actionButton.Text = Durango.Logic.Item.Inventory.ToCurrencyButtonText(T._("빠른 전송"), immediateReceivingCost, Currency.Gem);
			}
			else
			{
				_actionButton.Text = T._("빠른 전송");
			}
			_actionButton.Disabled = !flag;
			item = selected.Item;
		}
		else if (_completedWidget.List.SelectedList.Count > 0)
		{
			_buttonContainer.SetActive(value: true);
			_actionButton.Text = T._("완료 아이템 가져가기");
			_actionButton.Disabled = false;
			item = _completedWidget.List.LastSelectedItem;
		}
		else
		{
			_buttonContainer.SetActive(value: false);
		}
		_itemInfoWidget.Show(item);
	}

	private void OnClickActionButton()
	{
		if (_queueWidget.Selected.Item == null)
		{
			string[] array = new string[_completedWidget.List.SelectedList.Count];
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = _completedWidget.List.SelectedList[i].Id;
			}
			InventorySystem.TakeOutItems(_parent.Id, _parent.Tile, array);
			return;
		}
		string selectedId = _queueWidget.Selected.Item.Id;
		ReceiveCargoImmediately msg = default(ReceiveCargoImmediately);
		msg.EntityId = _parent.Id;
		msg.Tile = _parent.Tile;
		msg.ItemId = selectedId;
		InventorySystem.ReceiveCargoImmediately(msg, delegate(bool success)
		{
			if (success)
			{
				int num = -1;
				for (int j = 0; j < _receivingItems.Count; j++)
				{
					if (_receivingItems[j].Item.Id == selectedId)
					{
						num = j;
						break;
					}
				}
				if (num != -1)
				{
					ReceivingItem item = _receivingItems[num];
					double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
					double num2 = Math.Max(predictedServerTime, item.ReceivingAt) - Math.Max(predictedServerTime, item.WarpStartsAt);
					for (int k = num + 1; k < _receivingItems.Count; k++)
					{
						ReceivingItem value = _receivingItems[k];
						value.WarpStartsAt -= num2;
						value.ReceivingAt -= num2;
						_receivingItems[k] = value;
					}
					item.WarpStartsAt = 0.0;
					item.ReceivingAt = 0.0;
					_receivingItems.RemoveAt(num);
					_receivingItems.Insert(0, item);
					Refresh();
				}
			}
		});
	}

	private void OnDetailQueueButtonClick()
	{
		SetDetailPage(instant: false);
		OnSelectedQueueItem((_queueWidget.Selected.Item != null) ? _queueWidget.Selected.Item.Id : null);
	}

	private void OnUpdatePlayerInventory()
	{
		bool flag = false;
		List<ItemData> playerItemList = GameSystem<InventorySystem>.Instance().PlayerItemList;
		for (int i = 0; i < _receivedItems.Count; i++)
		{
			string id = _receivedItems[i].Id;
			if (playerItemList.Any((ItemData item) => item.Id == id))
			{
				_receivedItems.RemoveAt(i);
				i--;
				flag = true;
			}
		}
		if (flag)
		{
			Refresh();
		}
	}

	private static int RecevingItemComparison(ReceivingItem i1, ReceivingItem i2)
	{
		if (i1.ReceivingAt < i2.ReceivingAt)
		{
			return -1;
		}
		return 1;
	}
}
