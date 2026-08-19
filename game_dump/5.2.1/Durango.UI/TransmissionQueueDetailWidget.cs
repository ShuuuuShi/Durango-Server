using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Network;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class TransmissionQueueDetailWidget : AnimationWidget
{
	[SerializeField]
	private UILabel _sizeLabel;

	[SerializeField]
	private KInfiniteScrollView _itemList;

	private KInfiniteScrollView.View<ReceivingItem, TransmissionQueueDetailItem> _listView;

	private bool _isInit;

	public ItemData SelectedItem { get; private set; }

	public event Action<string> ItemSelected;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_listView = _itemList.Initialize(delegate(TransmissionQueueDetailItem widget, ReceivingItem data)
			{
				widget.Set(_listView.CurrentIndex, data);
				widget.Selected = SelectedItem != null && data.Item != null && data.Item.Id == SelectedItem.Id;
			}, delegate(TransmissionQueueDetailItem item)
			{
				item.Clicked = (Action)Delegate.Combine(item.Clicked, new Action(OnClickQueueItem));
			});
		}
	}

	private void Update()
	{
		if (_listView == null)
		{
			return;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		foreach (TransmissionQueueDetailItem item in _listView.List)
		{
			item.UpdateTimer(predictedServerTime);
		}
	}

	public void ResetData()
	{
		_itemList.ResetPosition();
		SelectItemWidget(null);
	}

	public void Set([NotNull] List<ReceivingItem> items, int capacity)
	{
		Init();
		int num = 0;
		string text = ((SelectedItem != null) ? SelectedItem.Id : null);
		int num2 = -1;
		for (int i = 0; i < items.Count; i++)
		{
			if (items[i].Item != null)
			{
				num += items[i].Item.Size;
				if (text != null && items[i].Item.Id == text)
				{
					num2 = i;
					break;
				}
			}
		}
		SelectedItem = ((num2 != -1) ? items[num2].Item : null);
		_sizeLabel.text = $"[FFD85B]{num} [B4B4B4]/ [CCCCCC]{capacity}";
		_listView.SetList(items);
		_itemList.Reposition();
		Update();
	}

	public void SelectItemWidget(string id)
	{
		SelectedItem = null;
		if (_listView == null || _listView.List == null)
		{
			return;
		}
		foreach (TransmissionQueueDetailItem item in _listView.List)
		{
			if (item.Data.Item != null)
			{
				if (item.Data.Item.Id == id)
				{
					item.Selected = true;
					SelectedItem = item.Data.Item;
				}
				else
				{
					item.Selected = false;
				}
			}
		}
	}

	private void OnClickQueueItem()
	{
		TransmissionQueueDetailItem transmissionQueueDetailItem = Selectable.Current as TransmissionQueueDetailItem;
		if (!(transmissionQueueDetailItem == null) && this.ItemSelected != null)
		{
			string obj = (transmissionQueueDetailItem.Selected ? null : ((transmissionQueueDetailItem.Data.Item != null) ? transmissionQueueDetailItem.Data.Item.Id : null));
			this.ItemSelected(obj);
		}
	}
}
