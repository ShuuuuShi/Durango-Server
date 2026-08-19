using System.Collections.Generic;
using ItemSystem;
using L10N;
using MarketData;
using Messages;
using UnityEngine;

public class PersonalMarketWiget : MonoBehaviour
{
	[SerializeField]
	private CommodityTabs _tabs;

	[SerializeField]
	private MarketInfoWidget _marketInfo;

	[SerializeField]
	private CommodityList _commodityList;

	[SerializeField]
	private SimilarItemsWidget _similarItems;

	[SerializeField]
	private CommodityListBottomBar _bottomBar;

	[SerializeField]
	private GameObject _noData;

	private int _selectedTab;

	private Market[] _markets;

	private readonly List<Commodities> _commoditiesList = new List<Commodities>();

	private ulong _owner;

	private ulong _defaultMarket;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_tabs.TabClicked += OnSelectTab;
			_bottomBar.BuyButtonClicked += OnBuyCommodity;
			_bottomBar.SimilarButtonClicked += OnSimilarItems;
			_commodityList.CommoditySelected += OnSelectCommodity;
		}
	}

	public void Set(ulong owner, ulong marketId)
	{
		Init();
		_owner = owner;
		_defaultMarket = marketId;
		((Component)_tabs).gameObject.SetActive(false);
		((Component)_marketInfo).gameObject.SetActive(false);
		((Component)_commodityList).gameObject.SetActive(false);
		_noData.gameObject.SetActive(false);
		_selectedTab = -1;
		ResetCommodities();
		GameSystem<MarketSystem>.Instance().GetPlayersMarkets(owner, OnMarkets);
	}

	private void OnMarkets(Markets markets)
	{
		int size = KUtility.GetSize(markets._Markets);
		if (size == 0)
		{
			((Component)_tabs).gameObject.SetActive(false);
			((Component)_marketInfo).gameObject.SetActive(false);
			((Component)_commodityList).gameObject.SetActive(false);
			_noData.gameObject.SetActive(true);
			return;
		}
		((Component)_tabs).gameObject.SetActive(true);
		((Component)_marketInfo).gameObject.SetActive(true);
		((Component)_commodityList).gameObject.SetActive(true);
		_noData.gameObject.SetActive(false);
		_markets = markets._Markets;
		_tabs.Set(markets._Markets);
		int num = 0;
		for (int i = 0; i < size; i++)
		{
			ulong id = markets._Markets[i].Id;
			RegistCommodities(id);
			if (id == _defaultMarket)
			{
				num = i;
			}
		}
		OnSelectTab(_markets[num]);
	}

	private void OnSelectTab(Market market)
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
			_tabs.SelectTab(num);
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
		_commodityList.Set(_commoditiesList[_selectedTab]);
	}

	private void OnSelectCommodity(Commodity commodity)
	{
		_bottomBar.Show(commodity);
	}

	private void OnBuyCommodity()
	{
		Commodity selected = _commodityList.Selected;
		UIManager.MessageBox.Show(T._("<t_stone></t_stone> {0:으로} <em>{1:을}</em> 구매합니다", selected.Price, selected.GetItem().Name), CommodityBought);
	}

	private void CommodityBought(bool ok)
	{
		if (ok)
		{
			Market market = _markets[_selectedTab];
			_commoditiesList[_selectedTab].Buy(market.Id, market.Tile, _commodityList.Selected);
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

	private void OnReceiveSimilarItems(ItemData item, Commodity[] similars)
	{
		ulong num = (_commodityList.Selected?.GetItem())?.Id ?? 0;
		if (num == item.Id)
		{
			_similarItems.Show(similars);
		}
	}
}
