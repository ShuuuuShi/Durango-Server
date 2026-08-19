using System;
using System.Collections.Generic;
using MarketData;
using Messages;
using Shared.Market;
using UnityEngine;

public class MyCommodityWidget : MonoBehaviour
{
	[SerializeField]
	private CommodityTabs _tabWidget;

	[SerializeField]
	private MarketInfoWidget _marketInfo;

	[SerializeField]
	private CommodityList _commodityList;

	[SerializeField]
	private GameObject _noData;

	[SerializeField]
	private GameObject _bototmBar;

	[SerializeField]
	private DefaultSelectableButton _refundButton;

	private int _selectedTab;

	private Market[] _markets;

	private readonly List<Commodities> _commoditiesList = new List<Commodities>();

	private bool _isOpen;

	private AnimationWidget _animWidget;

	public Market Market { get; set; }

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

	private void Awake()
	{
		DefaultSelectableButton refundButton = _refundButton;
		refundButton.Clicked = (Action)Delegate.Combine(refundButton.Clicked, new Action(OnClickRefundButton));
		_commodityList.CommoditySelected += OnSelectCommodity;
		_tabWidget.TabClicked += OnSelectMarketTab;
		_commodityList.SetFilter(RegionFilter);
	}

	public void OnEnable()
	{
		_bototmBar.gameObject.SetActive(false);
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
			_selectedTab = -1;
			((Component)_tabWidget).gameObject.SetActive(false);
			((Component)_marketInfo).gameObject.SetActive(false);
			((Component)_commodityList).gameObject.SetActive(false);
			_noData.gameObject.SetActive(false);
			ResetCommodities();
			GameSystem<MarketSystem>.Instance().GetPlayersMarkets(GameManager.PlayerId, OnPlayerMarkets);
		}
	}

	public void Close(bool instant = false)
	{
		if (instant || _isOpen)
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
	}

	private void OnPlayerMarkets(Markets markets)
	{
		if (!_isOpen)
		{
			return;
		}
		_markets = markets._Markets;
		int size = KUtility.GetSize(_markets);
		if (size == 0)
		{
			((Component)_tabWidget).gameObject.SetActive(false);
			((Component)_marketInfo).gameObject.SetActive(false);
			((Component)_commodityList).gameObject.SetActive(false);
			_noData.gameObject.SetActive(true);
			return;
		}
		((Component)_tabWidget).gameObject.SetActive(true);
		((Component)_marketInfo).gameObject.SetActive(true);
		((Component)_commodityList).gameObject.SetActive(true);
		_noData.gameObject.SetActive(false);
		_tabWidget.Set(_markets);
		for (int i = 0; i < size; i++)
		{
			RegistCommodities(_markets[i].Id);
		}
		OnSelectMarketTab(_markets[0]);
	}

	private void ResetCommodities()
	{
		for (int i = 0; i < _commoditiesList.Count; i++)
		{
			_commoditiesList[i].GoodsListUpdated -= OnUpdateGoodsList;
		}
		_commoditiesList.Clear();
	}

	private void RegistCommodities(ulong marketId)
	{
		int num = -1;
		for (int i = 0; i < _commoditiesList.Count; i++)
		{
			if (_commoditiesList[i].Request.Id == marketId)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			Commodities commodities = new Commodities();
			commodities.Request.Type = CommodityOwner.Market;
			commodities.Request.Id = marketId;
			commodities.GoodsListUpdated += OnUpdateGoodsList;
			_commoditiesList.Add(commodities);
		}
	}

	private void OnUpdateGoodsList()
	{
		if (_isOpen)
		{
			_commodityList.Set(_commoditiesList[_selectedTab]);
		}
	}

	private void OnSelectCommodity(Commodity commodity)
	{
		_bototmBar.gameObject.SetActive(commodity != null);
	}

	private void OnSelectMarketTab(Market market)
	{
		int num = -1;
		int i = 0;
		for (int size = KUtility.GetSize(_markets); i < size; i++)
		{
			if (_markets[i].Id == market.Id)
			{
				num = i;
				break;
			}
		}
		if (num != _selectedTab)
		{
			_selectedTab = num;
			_marketInfo.Set(market);
			_tabWidget.SelectTab(num);
			Commodities commodities = _commoditiesList[num];
			if (commodities.Goods.Count == 0)
			{
				_commodityList.SetLoading();
				commodities.Get(reset: true);
			}
			else
			{
				_commodityList.Set(commodities);
			}
		}
	}

	private bool RegionFilter(Commodity commodity)
	{
		return commodity.State == ProductState.Listed;
	}

	private void OnClickRefundButton()
	{
		if (_commodityList.Selected != null)
		{
			_commoditiesList[_selectedTab].Refund(Market.Id, Market.Tile, _commodityList.Selected);
		}
	}
}
