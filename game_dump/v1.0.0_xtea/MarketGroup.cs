using System;
using System.Text;
using ExploreData;
using ItemSystem;
using L10N;
using Messages;
using Shared.Economy;
using Shared.System;
using UnityEngine;

public class MarketGroup : UIBase
{
	[SerializeField]
	private UITitleWidget _titleWidget;

	[SerializeField]
	private MarketMenuWidget _marketMenus;

	[SerializeField]
	private UIWidget _mainContentsWidget;

	[SerializeField]
	private PersonalMarketWiget _personalMarket;

	[SerializeField]
	private CommodityListWidget _goodsList;

	[SerializeField]
	private SellItemWidget _sellItemWidget;

	[SerializeField]
	private MyCommodityWidget _myCommodityList;

	private string _titleFormat;

	private Market _market;

	private MarketMenuWidget.Tab _selectedTab;

	private ulong _seller;

	private bool _hasMainContents;

	public Market Market
	{
		get
		{
			return _market;
		}
		private set
		{
			_market = value;
			_goodsList.Market = value;
			_sellItemWidget.Market = value;
			_myCommodityList.Market = value;
		}
	}

	private void Awake()
	{
		_titleFormat = _titleWidget.GetTitle();
		base.OnClose();
	}

	private void Start()
	{
		_titleWidget.OnBack += Close;
		_titleWidget.OnClose += base.ForceClose;
		_marketMenus.TabClicked += MenuSelected;
		CommodityListWidget goodsList = _goodsList;
		goodsList.DepthChanged = (Action)Delegate.Combine(goodsList.DepthChanged, (Action)delegate
		{
			_titleWidget.ShowBackButton(_goodsList.HasDepth());
		});
		AddInteractionHandler();
	}

	private void OnEnable()
	{
		GameSystem<MarketSystem>.Instance().OnProductSold += OnProductSold;
	}

	private void OnDisable()
	{
		GameSystem<MarketSystem>.Instance().OnProductSold -= OnProductSold;
	}

	public void Open(Artifact market)
	{
		if ((Object)(object)market != (Object)null && market.FounderId != GameManager.PlayerId)
		{
			Open(market.FounderId, market.EntityId);
		}
		else
		{
			Open();
		}
		if ((Object)(object)market != (Object)null)
		{
			GameSystem<MarketSystem>.Instance().GetMarket(market.EntityId, OnMarket);
		}
	}

	public void OpenAndSearch(TagFilter[] tags, TagFilter[] materials)
	{
		Open();
		_goodsList.ResetAndShowSearchWidget(tags, materials);
	}

	private void OnMarket(Market market)
	{
		Market = market;
	}

	public void Open(ulong seller, ulong marketId = 0)
	{
		_hasMainContents = base.IsOpen;
		_seller = seller;
		Open();
		ShowPersonalMarketPage(marketId);
	}

	private void ShowMainPage(bool reset)
	{
		if (reset)
		{
			_goodsList.Open(instant: true);
			_sellItemWidget.Close(instant: true);
			_myCommodityList.Close(instant: true);
			_marketMenus.SelectTab(MarketMenuWidget.Tab.Buy);
			_selectedTab = MarketMenuWidget.Tab.Buy;
		}
		_marketMenus.Widget.alpha = 1f;
		_mainContentsWidget.alpha = 1f;
		((Component)_personalMarket).gameObject.SetActive(false);
		ExploreData.Region region = KSingleton<GameManager>.Instance().Region;
		_titleWidget.SetTitle(T._(_titleFormat, region.Name));
		_titleWidget.ShowBackButton(isShow: false);
	}

	private void ShowPersonalMarketPage(ulong marketId)
	{
		_marketMenus.Widget.alpha = 0f;
		_mainContentsWidget.alpha = 0f;
		((Component)_personalMarket).gameObject.SetActive(true);
		_personalMarket.Set(_seller, marketId);
		_titleWidget.SetTitle(T._("개인 가판대"));
		_titleWidget.ShowBackButton(_hasMainContents);
	}

	protected override bool OnOpen()
	{
		base.OnOpen();
		ShowMainPage(reset: true);
		return true;
	}

	protected override bool OnClose()
	{
		bool flag;
		if (((Component)_personalMarket).gameObject.activeSelf)
		{
			if (_hasMainContents)
			{
				flag = false;
				ShowMainPage(reset: false);
			}
			else
			{
				flag = true;
			}
		}
		else
		{
			flag = _goodsList.Back();
		}
		if (flag)
		{
			base.OnClose();
			Market = default(Market);
			return true;
		}
		return false;
	}

	private void OpenGoodsList()
	{
		_goodsList.Open();
		_sellItemWidget.Close();
		_myCommodityList.Close();
	}

	private void OpenSellItem()
	{
		_goodsList.Close();
		_sellItemWidget.Open();
		_myCommodityList.Close();
	}

	private void OpenMyList()
	{
		_goodsList.Close();
		_sellItemWidget.Close();
		_myCommodityList.Open();
	}

	private void MenuSelected(MarketMenuWidget.Tab menu)
	{
		switch (menu)
		{
		case MarketMenuWidget.Tab.Buy:
			if (_selectedTab == MarketMenuWidget.Tab.Buy)
			{
				ShowMainPage(reset: true);
				break;
			}
			_marketMenus.SelectTab(MarketMenuWidget.Tab.Buy);
			OpenGoodsList();
			break;
		case MarketMenuWidget.Tab.Sell:
			if (Market.Id == 0L)
			{
				UIManager.SystemMsg(T._("아이템을 판매하려면 가판대가 필요합니다"));
				_marketMenus.SelectTab(_selectedTab);
				return;
			}
			_marketMenus.SelectTab(MarketMenuWidget.Tab.Sell);
			OpenSellItem();
			break;
		case MarketMenuWidget.Tab.My:
			_marketMenus.SelectTab(MarketMenuWidget.Tab.My);
			OpenMyList();
			break;
		}
		_selectedTab = menu;
		_titleWidget.ShowBackButton(isShow: false);
	}

	private void AddInteractionHandler()
	{
		GameSystem<InteractionSystem>.Instance().AddInteractionHandler(Interaction.UseKiosk, delegate(InteractionObject obj)
		{
			Open(obj.GetTargetComponent<Artifact>());
		});
	}

	private void OnProductSold(ProductSold sold)
	{
		StringBuilder stringBuilder = new StringBuilder();
		for (int i = 0; i < sold.Items.Length; i++)
		{
			Item item = sold.Items[i];
			if (stringBuilder.Length != 0)
			{
				stringBuilder.Append(", ");
			}
			stringBuilder.Append(item.Name);
		}
		string text = T._("[eebb88]{0}[-] 아이템을 팔아 {1:을} 획득하였습니다.", stringBuilder, ItemSystem.Inventory.CurrencyFormat(sold.Price, Currency.TStone));
		UIManager.Popup.Alarm.ShowAlarm(text, "alarm_market", 4f);
		GameSystem<SocialSystem>.Instance().AddSystemChat(text, string.Empty, remainColor: true);
	}
}
