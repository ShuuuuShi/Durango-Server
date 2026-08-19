using System;
using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.UI.Control;
using NestedPrefab;
using UnityEngine;

namespace Durango.UI;

public class TransmissionCompletedWidget : AnimationWidget
{
	[SerializeField]
	private NestedPrefabLinker _itemListLinker;

	[SerializeField]
	private UILabel _sizeLabel;

	private ItemList _itemList;

	private bool _isInit;

	public ItemList List
	{
		get
		{
			Init();
			return _itemList;
		}
	}

	public event Action SelectedListUpdated;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_itemList = _itemListLinker.Object.GetComponent<ItemList>();
			_itemList.SelectableCount = -1;
			_itemList.FixedIconSize = true;
			ItemList itemList = _itemList;
			itemList.OnUpdateSelectItem = (Action)Delegate.Combine(itemList.OnUpdateSelectItem, new Action(OnUpdateSelectItem));
		}
	}

	public void Set(IList<ItemData> items, int capacity)
	{
		Init();
		int num = 0;
		int i = 0;
		for (int size = KUtility.GetSize(items); i < size; i++)
		{
			num += items[i].Size;
		}
		_sizeLabel.text = $"[FFD85B]{num} [B4B4B4]/ [CCCCCC]{capacity}";
		_itemList.SetItemList(items);
	}

	private void OnUpdateSelectItem()
	{
		if (this.SelectedListUpdated != null)
		{
			this.SelectedListUpdated();
		}
	}
}
