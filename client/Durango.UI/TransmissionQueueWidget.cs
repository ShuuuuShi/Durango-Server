using System;
using System.Collections.Generic;
using Durango.Network;
using Durango.UI.Control;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class TransmissionQueueWidget : AnimationWidget, IScreenResizeReceiver
{
	[SerializeField]
	private UILabel _sizeLabel;

	[SerializeField]
	private KInfiniteScrollView _itemList;

	[SerializeField]
	private UIWidget _queueListWidget;

	[SerializeField]
	private RectLayout _queueListLayout;

	[SerializeField]
	private Selectable _detailButton;

	[SerializeField]
	private UIWidget _moreButtonWidget;

	private ReceivingItem _selected;

	private KInfiniteScrollView.View<ReceivingItem, TransmissionQueueItem> _listView;

	private int _cellSize;

	private int _columnCount;

	private bool _isInit;

	public ReceivingItem Selected => _selected;

	public event Action<string> ItemSelected;

	public event Action DetailButtonClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_listView = _itemList.Initialize(delegate(TransmissionQueueItem widget, ReceivingItem data)
		{
			widget.Set(_listView.CurrentIndex, data);
			widget.Selected = Selected.Item != null && data.Item != null && data.Item.Id == Selected.Item.Id;
			if (widget.Widget.width != _cellSize)
			{
				widget.Widget.width = _cellSize;
				UIUtility.UpdateAnchors(widget.transform);
			}
		}, delegate(TransmissionQueueItem item)
		{
			item.Clicked = (Action)Delegate.Combine(item.Clicked, new Action(OnClickQueueItem));
		});
		Selectable detailButton = _detailButton;
		detailButton.Clicked = (Action)Delegate.Combine(detailButton.Clicked, (Action)delegate
		{
			if (this.DetailButtonClicked != null)
			{
				this.DetailButtonClicked();
			}
		});
	}

	private void Update()
	{
		if (_listView == null)
		{
			return;
		}
		double predictedServerTime = Connections.Frontend.GetPredictedServerTime();
		foreach (TransmissionQueueItem item in _listView.List)
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
		string text = ((_selected.Item != null) ? _selected.Item.Id : null);
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
		if (num2 == -1)
		{
			_selected.Item = null;
		}
		else
		{
			_selected = items[num2];
		}
		_sizeLabel.text = $"[FFD85B]{num} [B4B4B4]/ [CCCCCC]{capacity}";
		while (items.Count < _columnCount)
		{
			items.Add(default(ReceivingItem));
		}
		_listView.SetList(items);
		_itemList.Reposition();
		Update();
	}

	public void SelectItemWidget(string id)
	{
		_selected.Item = null;
		if (_listView == null || _listView.List == null)
		{
			return;
		}
		foreach (TransmissionQueueItem item in _listView.List)
		{
			if (item.Data.Item != null)
			{
				if (item.Data.Item.Id == id)
				{
					item.Selected = true;
					_selected = item.Data;
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
		TransmissionQueueItem transmissionQueueItem = Selectable.Current as TransmissionQueueItem;
		if (!(transmissionQueueItem == null) && this.ItemSelected != null)
		{
			string obj = (transmissionQueueItem.Selected ? null : ((transmissionQueueItem.Data.Item != null) ? transmissionQueueItem.Data.Item.Id : null));
			this.ItemSelected(obj);
		}
	}

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		float num = (float)base.Widget.width / 111f;
		_columnCount = Mathf.Max(1, Mathf.RoundToInt(num));
		float num2 = num / (float)_columnCount;
		_cellSize = Mathf.RoundToInt(111f * num2);
		_columnCount -= 2;
		_moreButtonWidget.width = _cellSize * 2;
		_queueListLayout.UpdateLayout(base.Widget.width, base.Widget.height);
		if (_listView != null)
		{
			_listView.NodeResize(new Point2(_cellSize, _queueListWidget.height));
		}
	}
}
