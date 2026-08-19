using System;
using Durango.Logic.Item;
using Durango.Logic.Market;
using Durango.Logic.Notification;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI;

public class MarketHistoryWidget : AnimationWidget, IUIInitializable, IScreenResizeReceiver
{
	private static readonly ProductType[] ProductTabsOrder = new ProductType[5]
	{
		ProductType.Sold,
		ProductType.Purchased,
		ProductType.Registered,
		ProductType.Expired,
		ProductType.Favorites
	};

	[SerializeField]
	private NestedPrefabLinker _tabLinker;

	[SerializeField]
	private CommodityList _commodityList;

	[SerializeField]
	private AnimationWidget _buttonContainer;

	[SerializeField]
	private SelectableButton _actionButton;

	[SerializeField]
	private SelectableButton _searchButton;

	[SerializeField]
	private MarketFavoritesButton _favoritesButton;

	[SerializeField]
	private GameObject _receiveButtonBar;

	[SerializeField]
	private SelectableButton _receiveAllButton;

	[SerializeField]
	private SelectableButton _searchButtonInReceiveBar;

	[SerializeField]
	private RectLayout _layout;

	private ProductType _currentTab;

	private HorizontalTabList _tabList;

	private readonly Durango.Logic.Market.Commodities _commodities = new Durango.Logic.Market.Commodities();

	private bool _isOpen;

	public bool IsOpenedInSoldTab => _isOpen && _currentTab == ProductType.Sold;

	void IUIInitializable.Init()
	{
		_commodityList.Init();
		_commodityList.SetScrollEndPadding(_buttonContainer.Widget.height);
		_commodityList.CommoditySelected += OnCommoditySelected;
		_commodities.GoodsListUpdated += OnUpdateGoodsList;
		_commodities.Request.Condition = _commodityList.GetSortCondition();
		SelectableButton actionButton = _actionButton;
		actionButton.Clicked = (Action)Delegate.Combine(actionButton.Clicked, new Action(OnActionButtonClicked));
		SelectableButton searchButton = _searchButton;
		searchButton.Clicked = (Action)Delegate.Combine(searchButton.Clicked, new Action(OnSearchButtonClicked));
		SelectableButton searchButtonInReceiveBar = _searchButtonInReceiveBar;
		searchButtonInReceiveBar.Clicked = (Action)Delegate.Combine(searchButtonInReceiveBar.Clicked, new Action(OnSearchButtonClicked));
		SelectableButton receiveAllButton = _receiveAllButton;
		receiveAllButton.Clicked = (Action)Delegate.Combine(receiveAllButton.Clicked, new Action(OnReceiveAllButtonClicked));
		_tabList = _tabLinker.Object.GetComponent<HorizontalTabList>();
		_tabList.BeginLoad();
		for (int i = 0; i < ProductTabsOrder.Length; i++)
		{
			ProductType productType = ProductTabsOrder[i];
			string text = productType switch
			{
				ProductType.Registered => T._("등록된 물품"), 
				ProductType.Purchased => T._("구매 내역"), 
				ProductType.Sold => T._("판매 내역"), 
				ProductType.Expired => T._("만료된 물품"), 
				ProductType.Favorites => T._("찜 목록"), 
				_ => productType.ToString(), 
			};
			_tabList.AddText(text);
		}
		_tabList.EndLoadByFixedSize(150);
		_tabList.Clicked += delegate(int index)
		{
			SelectTab(ProductTabsOrder[index]);
		};
	}

	public void Open(ProductType type)
	{
		_isOpen = true;
		SetAlpha(1f);
		base.gameObject.SetActive(value: true);
		SelectTab(type);
	}

	public void Close(bool instant = false)
	{
		if (instant || _isOpen)
		{
			_isOpen = false;
			if (instant)
			{
				base.gameObject.SetActive(value: false);
				SetAlpha(0f, useTween: false);
			}
			else
			{
				base.Delay = 0f;
				base.Alpha = 0f;
			}
		}
	}

	private void SelectTab(ProductType tabType)
	{
		_currentTab = tabType;
		RefreshReceiveButtonBar();
		_tabList.Select(ProductTabsOrder.IndexOf(tabType));
		_commodities.Request.Type = tabType;
		_commodityList.SetLoading();
		_commodityList.SetProductType(_commodities.Request.Type);
		_commodities.Get(reset: true);
	}

	public void RefreshReceiveButtonBar()
	{
		bool flag = _currentTab == ProductType.Sold;
		if (flag != _receiveButtonBar.activeSelf)
		{
			_receiveButtonBar.SetActive(flag);
			_layout.UpdateLayout();
			UIUtility.UpdateAnchors(base.transform);
		}
		if (flag)
		{
			_receiveAllButton.Disabled = !GameSystem<MarketSystem>.Instance().HasCollectiblePayment;
		}
	}

	public void SetNotification(ProductType type, bool on, Durango.Logic.Notification.Type notificationType)
	{
		int num = ProductTabsOrder.IndexOf(type);
		if (num != -1)
		{
			_tabList.SetNotification(num, on, notificationType);
		}
	}

	public void PaymentReceived(string productId = null)
	{
		_commodityList.PaymentReceived(productId);
		RefreshReceiveButtonBar();
	}

	private void OnUpdateGoodsList()
	{
		if (_isOpen)
		{
			_commodityList.Set(_commodities);
			OnCommoditySelected(_commodityList.Selected);
		}
	}

	private void OnCommoditySelected(Commodity commodity)
	{
		if (commodity == null)
		{
			ShowButton(show: false);
			_searchButtonInReceiveBar.Disabled = true;
			return;
		}
		ProductType currentTab = _currentTab;
		if (currentTab == ProductType.Sold)
		{
			_searchButtonInReceiveBar.Disabled = false;
			return;
		}
		ShowButton(show: true);
		bool flag = currentTab == ProductType.Registered || currentTab == ProductType.Expired || currentTab == ProductType.Favorites;
		_actionButton.gameObject.SetActive(flag);
		if (flag)
		{
			switch (currentTab)
			{
			case ProductType.Registered:
				_actionButton.Text = T._("등록 취소");
				break;
			case ProductType.Expired:
				_actionButton.Text = T._("받기");
				break;
			case ProductType.Favorites:
				_actionButton.Text = T._("<t_stone> {0} 구매", commodity.Price.ToString("N0", T.Culture));
				break;
			}
		}
		_favoritesButton.Set((_currentTab != ProductType.Favorites) ? null : commodity, _commodityList.UpdateCommodity);
	}

	private void ShowButton(bool show)
	{
		if (show)
		{
			_buttonContainer.gameObject.SetActive(value: true);
			_buttonContainer.Alpha = 1f;
		}
		else
		{
			_buttonContainer.Alpha = 0f;
		}
	}

	private void OnActionButtonClicked()
	{
		Commodity selected = _commodityList.Selected;
		if (selected == null)
		{
			return;
		}
		switch (_currentTab)
		{
		case ProductType.Registered:
			_commodities.Unregister(selected);
			break;
		case ProductType.Expired:
			_commodities.Withdraw(selected);
			break;
		case ProductType.Favorites:
		{
			ItemData item = selected.GetItem();
			if (item != null)
			{
				UIManager.MessageBox.Show(T._("{0:으로} <em>{1}</em>{1:-을} 구매합니다", Durango.Logic.Item.Inventory.CurrencyFormat(selected.Price, Currency.TStone), item.Name), (Action)delegate
				{
					_commodities.Buy(selected);
				}, (string)null);
			}
			break;
		}
		case ProductType.Purchased:
		case ProductType.Sold:
			break;
		}
	}

	private void OnSearchButtonClicked()
	{
		Commodity selected = _commodityList.Selected;
		if (selected != null)
		{
			ItemData item = selected.GetItem();
			if (item != null)
			{
				MarketGroup marketGroup = UIManager.FindScript<MarketGroup>();
				marketGroup.Close();
				marketGroup.OpenAndSearch(item.PrototypeId);
			}
		}
	}

	private void OnReceiveAllButtonClicked()
	{
		MarketSystem.Send(default(MarketCollectAllPayments)).On<Products>(delegate
		{
			PaymentReceived();
		});
	}

	void IScreenResizeReceiver.OnChangeScreenSize()
	{
		_layout.UpdateLayout();
	}
}
