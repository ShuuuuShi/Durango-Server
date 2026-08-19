using System;
using System.Collections.Generic;
using MarketData;
using UnityEngine;

public class CommodityList : MonoBehaviour
{
	[SerializeField]
	private UIWidget _commodityListContainer;

	[SerializeField]
	private ListScroll _commodityList;

	[SerializeField]
	private GameObject _searchFilterButton;

	[SerializeField]
	private SearchFilterInfoWidget _searchFilterWidget;

	[SerializeField]
	private GameObject _noData;

	[SerializeField]
	private LoadingIconControl _loadingIcon;

	private int _commodityListTopMargin;

	private Func<Commodity, bool> _filterFunc;

	private Commodities _list;

	private ListScroll.View<Commodity, CommodityNode> _commodityView;

	private List<Commodity> _filteredList = new List<Commodity>();

	private UIWidget _widget;

	private bool _isInit;

	public UIWidget Widget => (!((Object)(object)_widget == (Object)null)) ? _widget : (_widget = ((Component)this).GetComponent<UIWidget>());

	public Commodity Selected { get; private set; }

	public ListScroll ScrollView => _commodityList;

	public event Action<Commodity> CommoditySelected;

	public event Action SearchFilterClicked;

	private void OnDisable()
	{
		OnSelectCommodity(null);
	}

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_commodityListTopMargin = _commodityListContainer.topAnchor.absolute;
			_commodityView = _commodityList.Initialize<Commodity, CommodityNode>((Action<CommodityNode, Commodity>)delegate(CommodityNode node, Commodity data)
			{
				node.Set(data);
				node.Select = Selected != null && data != null && Selected.Id == data.Id;
			}, (Action<CommodityNode>)delegate(CommodityNode node)
			{
				node.Clicked = OnClickCommodityNode;
			});
			if (!((Object)(object)_searchFilterButton != (Object)null))
			{
			}
		}
	}

	public void SetFilter(Func<Commodity, bool> filter)
	{
		Init();
		_filterFunc = filter;
		UpdateList();
	}

	private void OnClickCommodityNode()
	{
		CommodityNode commodityNode = Selectable.Current as CommodityNode;
		if (!((Object)(object)commodityNode == (Object)null))
		{
			Commodity commodity = commodityNode.Data;
			if (commodity == Selected)
			{
				commodity = null;
			}
			OnSelectCommodity(commodity);
			int index = _commodityView.IndexOf(commodityNode);
			_commodityList.MoveToVisibleArea(index, instant: false);
		}
	}

	private void OnSelectCommodity(Commodity commodity)
	{
		ulong num = ((Selected != null) ? Selected.Id : 0);
		Selected = null;
		if (commodity != null)
		{
			for (int i = 0; i < _list.Goods.Count; i++)
			{
				if (_list.Goods[i].Id == commodity.Id)
				{
					Selected = _list.Goods[i];
				}
			}
		}
		List<CommodityNode> list = ((_commodityView != null) ? _commodityView.List : null);
		ulong num2 = ((Selected != null) ? Selected.Id : 0);
		int j = 0;
		for (int size = KUtility.GetSize(list); j < size; j++)
		{
			list[j].Select = list[j].Data != null && list[j].Data.Id == num2;
		}
		if (num != num2 && this.CommoditySelected != null)
		{
			this.CommoditySelected(commodity);
		}
	}

	public void SetLoading()
	{
		((Component)_commodityList.ScrollView).GetComponent<UIRect>().alpha = 0f;
		if ((Object)(object)_noData != (Object)null)
		{
			_noData.SetActive(false);
		}
		if ((Object)(object)_loadingIcon != (Object)null)
		{
			((Component)_loadingIcon).gameObject.SetActive(true);
		}
		OnSelectCommodity(null);
	}

	public void Set(Commodities commdities)
	{
		Init();
		_list = commdities;
		if ((Object)(object)_searchFilterWidget != (Object)null)
		{
			((Component)_searchFilterWidget).gameObject.SetActive(false);
		}
		_commodityListContainer.topAnchor.absolute = ((!((Object)(object)_searchFilterWidget == (Object)null) && !((Component)_searchFilterWidget).gameObject.activeSelf) ? (_commodityListTopMargin + _searchFilterWidget.Widget.height) : _commodityListTopMargin);
		UIUtility.UpdateAnchors(((Component)_commodityListContainer).transform);
		_commodityList.PanelResized();
		UpdateList();
	}

	private void UpdateList()
	{
		List<Commodity> list;
		if (_list == null)
		{
			list = null;
		}
		else if (_filterFunc == null)
		{
			list = _list.Goods;
		}
		else
		{
			_filteredList.Clear();
			for (int i = 0; i < _list.Goods.Count; i++)
			{
				if (_filterFunc(_list.Goods[i]))
				{
					_filteredList.Add(_list.Goods[i]);
				}
			}
			list = _filteredList;
		}
		_commodityView.SetList(list);
		OnSelectCommodity(Selected);
		((Component)_commodityList.ScrollView).GetComponent<UIRect>().alpha = 1f;
		if ((Object)(object)_noData != (Object)null)
		{
			_noData.SetActive(KUtility.GetSize(list) == 0);
		}
		if ((Object)(object)_loadingIcon != (Object)null)
		{
			((Component)_loadingIcon).gameObject.SetActive(false);
		}
		_commodityList.Reposition();
	}
}
