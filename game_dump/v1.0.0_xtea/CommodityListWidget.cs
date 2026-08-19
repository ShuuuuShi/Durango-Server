using System;
using System.Collections.Generic;
using ItemSystem;
using L10N;
using MarketData;
using Messages;
using Shared.Market;
using UnityEngine;

public class CommodityListWidget : MonoBehaviour
{
	public Action DepthChanged;

	[SerializeField]
	private MarketGoodsCategoryWidget _categoryWidget;

	[SerializeField]
	private MarketSearchWidget _searchWidget;

	[SerializeField]
	private CommodityList _commodityList;

	[SerializeField]
	private SimilarItemsWidget _similarItems;

	[SerializeField]
	private CommodityListBottomBar _bottomBar;

	private bool _isOpen;

	private bool _isOpenGoodsList;

	private Commodities _commodities = new Commodities();

	private AnimationWidget _animWidget;

	private bool _init;

	private AnimationWidget AnimWidget
	{
		get
		{
			if ((Object)(object)_animWidget == (Object)null)
			{
				_animWidget = ((Component)this).GetComponent<AnimationWidget>();
			}
			return _animWidget;
		}
	}

	public Market Market { get; set; }

	private void Awake()
	{
		Init();
	}

	private void Init()
	{
		if (!_init)
		{
			_init = true;
			_commodities.OnRequestGoodsList += OnRequestGoodsList;
			_commodities.GoodsListUpdated += OnUpdatedGoodsList;
			ListScroll scrollView = _commodityList.ScrollView;
			scrollView.DragFinishedOnLast = (Action)Delegate.Combine(scrollView.DragFinishedOnLast, new Action(ScrollViewDragFinishedOnLast));
			_categoryWidget.CategorySelected += OnSelectCategory;
			_categoryWidget.SearchSelected += OnSearchAndReset;
			_commodityList.SearchFilterClicked += OnSearch;
			_commodityList.CommoditySelected += OnSelectCommodity;
			_searchWidget.Searched += ShowGoodsList;
			_bottomBar.BuyButtonClicked += OnBuyCommodity;
			_bottomBar.SimilarButtonClicked += OnSimilarItems;
			_bottomBar.PersonalMarketButtonClicked += OnPersonalMarket;
			((Component)_commodityList).gameObject.SetActive(true);
			_commodityList.Widget.alpha = 0f;
			_commodityList.SetFilter((Commodity commodity) => commodity.State == ProductState.Listed);
		}
	}

	public void Open(bool instant = false)
	{
		if (instant || !_isOpen)
		{
			_isOpen = true;
			((Component)this).gameObject.SetActive(true);
			if (instant)
			{
				AnimWidget.SetAlpha(1f, useTween: false);
			}
			else
			{
				AnimWidget.Delay = 0.2f;
				AnimWidget.Alpha = 1f;
			}
			ShowCategory();
		}
	}

	public bool HasDepth()
	{
		if (_commodityList.Widget.alpha > 0f)
		{
			return true;
		}
		if (_searchWidget.IsOpen)
		{
			return true;
		}
		return false;
	}

	public bool Back(bool instant = false)
	{
		if (!instant && !_isOpen)
		{
			return true;
		}
		if (_searchWidget.IsOpen)
		{
			if (_searchWidget.Close(all: false))
			{
				if (_isOpenGoodsList)
				{
					ShowGoodsList();
				}
				else
				{
					ShowCategory();
				}
			}
			return false;
		}
		if (_commodityList.Widget.alpha > 0f)
		{
			ShowCategory();
			return false;
		}
		Close(instant);
		return true;
	}

	public void Close(bool instant = false)
	{
		_isOpen = false;
		if (instant)
		{
			((Component)this).gameObject.SetActive(false);
			AnimWidget.SetAlpha(0f, useTween: false);
		}
		else
		{
			AnimWidget.Delay = 0f;
			AnimWidget.Alpha = 0f;
		}
	}

	private void ShowCategory()
	{
		_categoryWidget.Show();
		_commodityList.Widget.alpha = 0f;
		_searchWidget.Close(all: true);
		_isOpenGoodsList = false;
		if (DepthChanged != null)
		{
			DepthChanged();
		}
	}

	private void ShowGoodsList()
	{
		_categoryWidget.Hide();
		_commodityList.Widget.alpha = 1f;
		_searchWidget.Close(all: true);
		_isOpenGoodsList = true;
		if (DepthChanged != null)
		{
			DepthChanged();
		}
	}

	private void ShowSearchWidget()
	{
		_categoryWidget.Hide();
		_commodityList.Widget.alpha = 0f;
		_searchWidget.Open(_commodities);
		if (DepthChanged != null)
		{
			DepthChanged();
		}
	}

	private void ScrollViewDragFinishedOnLast()
	{
		_commodities.Get(reset: false);
	}

	private void OnRequestGoodsList(bool isReset)
	{
		if (_isOpen && isReset)
		{
			_commodityList.SetLoading();
		}
	}

	private void OnUpdatedGoodsList()
	{
		if (_isOpen)
		{
			_commodityList.Set(_commodities);
		}
	}

	private void OnBuyCommodity()
	{
		Commodity selected = _commodityList.Selected;
		UIManager.MessageBox.Show(T._("<t_stone></t_stone> {0:으로} <em>{1:을}</em> 구매합니다", selected.Price.ToString("N0"), selected.GetItem().Name), CommodityBought);
	}

	private void CommodityBought(bool ok)
	{
		if (ok)
		{
			_commodities.Buy(Market.Id, Market.Tile, _commodityList.Selected);
		}
	}

	private void OnSimilarItems()
	{
		ItemData itemData = _commodityList.Selected?.GetItem();
		if ((Object)(object)_similarItems != (Object)null && itemData != null)
		{
			_similarItems.Loading();
			GameSystem<MarketSystem>.Instance().GetSimilarProducts(itemData, 3, OnReceiveSimilarItems);
		}
	}

	private void OnPersonalMarket()
	{
		Commodity selected = _commodityList.Selected;
		if (selected != null)
		{
			MarketGroup marketGroup = UIManager.FindScript<MarketGroup>();
			marketGroup.Open(selected.SellerId, selected.MarketId);
		}
	}

	private void OnReceiveSimilarItems(ItemData item, Commodity[] similars)
	{
		ulong num = (_commodityList.Selected?.GetItem())?.Id ?? 0;
		if (num == item.Id)
		{
			_similarItems.Show(similars);
		}
	}

	private void OnSelectCategory(string category, string[] prototypes)
	{
		FilterOption filter = _commodities.Filter;
		filter.Reset();
		if (!string.IsNullOrEmpty(category))
		{
			if (filter.Prototype == null)
			{
				filter.Prototype = new List<RangeOption>();
			}
			int i = 0;
			for (int size = KUtility.GetSize(prototypes); i < size; i++)
			{
				filter.Prototype.Add(new RangeOption
				{
					Key = prototypes[i]
				});
			}
		}
		_commodities.Request.Type = CommodityOwner.Region;
		_commodities.Request.Id = KSingleton<GameManager>.Instance().Region.Id;
		_commodities.Get(reset: true);
		ShowGoodsList();
	}

	public void ResetAndShowSearchWidget(TagFilter[] tags, TagFilter[] materials)
	{
		_commodities.Reset();
		if (KUtility.GetSize(tags) > 0)
		{
			_commodities.Filter.Tags = new List<RangeOption>();
			TagFilter tagFilter = tags[0];
			_commodities.Filter.Tags.Add(new RangeOption
			{
				Key = tagFilter.TagId,
				Min = tagFilter.RequiredLevel
			});
			if (KUtility.GetSize(materials) > 0)
			{
				TagFilter tagFilter2 = materials[0];
				_commodities.Filter.Tags.Add(new RangeOption
				{
					Key = tagFilter2.TagId,
					Min = tagFilter2.RequiredLevel
				});
			}
		}
		ShowSearchWidget();
		_searchWidget.Search();
	}

	private void OnSearchAndReset()
	{
	}

	private void OnSearch()
	{
		ShowSearchWidget();
	}

	private void OnSelectCommodity(Commodity commodity)
	{
		_bottomBar.Show(commodity);
	}
}
