using System;
using System.Collections.Generic;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class ShopCommodityContentsList : UIWidget
{
	public Action<ContentDescription> Clicked;

	[SerializeField]
	private KScrollView _itemList;

	private IList<ContentDescription> _items;

	private bool _isDirty;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_itemList.Nodes.Init(delegate(GameObject obj)
			{
				SelectableWidget component = obj.GetComponent<SelectableWidget>();
				component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(OnClickItem));
			});
		}
	}

	protected override void OnUpdate()
	{
		base.OnUpdate();
		if (Application.isPlaying && _isDirty && IsItemsLoaded())
		{
			SetList(_items);
		}
	}

	private bool IsItemsLoaded()
	{
		if (_items == null)
		{
			return true;
		}
		foreach (ContentDescription item in _items)
		{
			if (!item.IsLoaded)
			{
				return false;
			}
		}
		return true;
	}

	public void Set(IList<ContentDescription> items)
	{
		Init();
		_items = items;
		_isDirty = true;
		if (IsItemsLoaded())
		{
			SetList(items);
			return;
		}
		base.visible = false;
		if (items == null)
		{
			return;
		}
		foreach (ContentDescription item in items)
		{
			item.Load();
		}
	}

	private void SetList(IList<ContentDescription> items)
	{
		_isDirty = false;
		base.visible = true;
		_itemList.Nodes.Set(KUtility.GetSize(items));
		for (int i = 0; i < _itemList.Nodes.Count; i++)
		{
			ShopCommodityContentItem component = _itemList.Nodes[i].GetComponent<ShopCommodityContentItem>();
			component.Set(items[i]);
		}
		_itemList.ResetPosition();
	}

	public void SelectItem(int index)
	{
		Init();
		for (int i = 0; i < _itemList.Nodes.Count; i++)
		{
			GameObject gameObject = _itemList.Nodes[i];
			gameObject.GetComponent<SelectableWidget>().Selected = i == index;
		}
	}

	private void OnClickItem()
	{
		GameObject obj = Selectable.Current.gameObject;
		int num = _itemList.Nodes.IndexOf(obj);
		if (num != -1)
		{
			ContentDescription obj2 = _items.Get(num);
			if (Clicked != null)
			{
				Clicked(obj2);
			}
		}
	}
}
