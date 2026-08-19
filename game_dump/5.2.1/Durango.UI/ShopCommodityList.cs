using System;
using System.Collections.Generic;
using Durango.Logic.Shop;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using NestedPrefab;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class ShopCommodityList : ShopCommodityListBase
{
	[SerializeField]
	private NestedPrefabLinker _subTabLinker;

	[SerializeField]
	protected NodesScrollView ScrollList;

	private List<ShopCategory> _categories;

	private HorizontalTabList _subTabList;

	protected override void OnInit()
	{
		_subTabList = _subTabLinker.Object.GetComponent<HorizontalTabList>();
		_subTabList.Clicked += delegate(int index)
		{
			if (_categories != null)
			{
				ShopCategory shopCategory = _categories.Get(index);
				if (shopCategory != null && CategorySelected != null)
				{
					CategorySelected(shopCategory);
				}
			}
		};
		ScrollList.Nodes.Init(delegate(GameObject obj)
		{
			ShopCommodityWidget component = obj.GetComponent<ShopCommodityWidget>();
			component.Clicked = (Action)Delegate.Combine(component.Clicked, new Action(ItemClicked));
		});
	}

	public override void RefreshCategoryNotification()
	{
		if (_categories != null)
		{
			for (int i = 0; i < _categories.Count; i++)
			{
				ShopCategory cat = _categories[i];
				base.Parent.GetCategoryNotifiaction(cat, out var on, out var type);
				_subTabList.SetNotification(i, on, type);
			}
		}
	}

	public override void SetList(List<Durango.Logic.Shop.Commodity> list, bool reset)
	{
		base.SetList(list, reset);
		ListObjectPool nodes = ScrollList.Nodes;
		nodes.BeginLoad();
		if (list != null)
		{
			foreach (Durango.Logic.Shop.Commodity item in list)
			{
				nodes.GetNext().GetComponent<ShopCommodityWidget>().Set(item);
			}
		}
		nodes.EndLoad();
		ScrollList.Reposition(reset, !reset);
	}

	public override void SelectAndMoveTo(string id)
	{
		int num = IndexOf(id);
		if (num != -1)
		{
			ScrollList.MoveToNode(num, instant: true);
		}
	}

	public override void SetSubCategories(List<ShopCategory> categories, ShopCategory selected)
	{
		_categories = categories;
		if (KUtility.GetSize(categories) > 0)
		{
			_subTabLinker.gameObject.SetActive(value: true);
			_subTabList.BeginLoad();
			foreach (ShopCategory category in categories)
			{
				_subTabList.AddText(category.Name.ToString());
			}
			_subTabList.EndLoadByFixedSize(200);
			_subTabList.Select(categories.IndexOf(selected));
		}
		else
		{
			_subTabLinker.gameObject.SetActive(value: false);
		}
	}

	private void ItemClicked()
	{
		int num = ScrollList.Nodes.IndexOf(Selectable.Current.gameObject);
		if (num != -1)
		{
			OnItemClicked(base.CurrentList[num]);
		}
	}

	protected virtual void OnItemClicked(Durango.Logic.Shop.Commodity item)
	{
		if (Selected != null)
		{
			Selected(item.Id);
		}
	}

	protected int IndexOf(string id)
	{
		int result = -1;
		int i = 0;
		for (int size = KUtility.GetSize(base.CurrentList); i < size; i++)
		{
			if (base.CurrentList[i].Id == id)
			{
				result = i;
				break;
			}
		}
		return result;
	}
}
