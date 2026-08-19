using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Cutscene;
using Durango.Logic.Clusters;
using Durango.Logic.Item;
using Durango.Logic.Notification;
using Durango.Logic.Shop;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Purchaser;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

[Uri("Shop")]
public class ShopGroup : UIBase, INotificationable
{
	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private ShopTabList _tabList;

	[SerializeField]
	private PromotionBannerList _promotionWidget;

	[SerializeField]
	private UIWidget _mainWidget;

	[SerializeField]
	private RectLayoutComponent _mainLayout;

	[SerializeField]
	private PurchasedPage _purchasedPage;

	[SerializeField]
	private UIWidget _commoditiesPageWidget;

	[SerializeField]
	private ShopCommoditiesPage _commoditiesPage;

	[SerializeField]
	private PurchasesPage _purchasesPage;

	[SerializeField]
	private CurrencyWidget _currencyBase;

	private bool _isPurchasePage;

	private ListObjectPool<CurrencyWidget> _currencyWidgets;

	private readonly Dictionary<ShopCategory, List<ShopCategory>> _subCategories = new Dictionary<ShopCategory, List<ShopCategory>>();

	private ShopCategory _selectedMainCategory;

	private ShopCategory _selectedSubCategory;

	private ShopCategory _lastSelectedCategory;

	private readonly List<Durango.Logic.Shop.Commodity> _bufferCommodities = new List<Durango.Logic.Shop.Commodity>();

	private readonly List<Durango.Logic.Shop.Commodity> _filteredCommodities = new List<Durango.Logic.Shop.Commodity>();

	private bool _isBuying;

	private bool _isCheckingBoughtItems;

	private string _reservedSelectCommodity;

	private readonly Durango.Logic.Notification.Container _notification = new Durango.Logic.Notification.Container();

	private readonly Toggle _purchsesNotifiaction = new Toggle(Durango.Logic.Notification.Type.Important);

	private readonly Toggle _acceptableSubPurchaseNotifiaction = new Toggle(Durango.Logic.Notification.Type.Important);

	private readonly Toggle _hasNewCommodityNotification = new Toggle(Durango.Logic.Notification.Type.Important);

	private readonly HashSet<ShopCategory> _hasNewCommodityCategories = new HashSet<ShopCategory>();

	private Durango.Logic.Shop.Commodity _buyReservedCommodity;

	public Notification Notification => _notification;

	private void Awake()
	{
		_openCloseSound = UISound.GroupType.Shop;
	}

	private void Start()
	{
		_titleWidget.Object.SetTitle(T._("상점"));
		_tabList.Selected = delegate(ShopCategory type)
		{
			Open(type);
		};
		ShopTabList tabList = _tabList;
		tabList.PurchaseSelected = (Action)Delegate.Combine(tabList.PurchaseSelected, new Action(OpenPurchases));
		_currencyWidgets = new ListObjectPool<CurrencyWidget>();
		_currencyWidgets.BaseObject = _currencyBase;
		_currencyWidgets.UseBase = true;
		_currencyWidgets.Clear();
		_notification.AddChild(_purchsesNotifiaction);
		_notification.AddChild(_acceptableSubPurchaseNotifiaction);
		_notification.AddChild(_hasNewCommodityNotification);
		_purchsesNotifiaction.Changed += UpdatePurchaseNotification;
		_acceptableSubPurchaseNotifiaction.Changed += RefreshNotification;
		UpdatePurchaseNotification();
		base.OnOpenSucceed += OnOpened;
		base.OnCloseSucceed += OnClosed;
		GameSystem<ShopSystem>.Instance().PurchasesUpdated += OnUpdatePurchases;
		GameSystem<ShopSystem>.Instance().AcceptableSubPurchasesUpdated += OnAcceptableSubPurchasesUpdated;
		GameSystem<ShopSystem>.Instance().UserFirstPurchaseHistoryUpdated += OnUserFirstPurchaseHistoryUpdated;
		GameSystem<ShopSystem>.Instance().AcceptableSubPurchaseItem += OnNewAcceptableSubPurchaseItem;
		GameSystem<ShopSystem>.Instance().ReadCommoditiesChanged += OnChangeReadCommodities;
		GameSystem<InventorySystem>.Instance().WalletUpdated += OnUpdateWallet;
		Durango.Utils.Singleton<PetManager>.Instance().OnDomesticationResult += OnDomesticationResult;
		ShopCommoditiesPage commoditiesPage = _commoditiesPage;
		commoditiesPage.ListChanged = (Action<List<Durango.Logic.Shop.Commodity>>)Delegate.Combine(commoditiesPage.ListChanged, new Action<List<Durango.Logic.Shop.Commodity>>(UpdateCurrencyWidgets));
		ShopCommoditiesPage commoditiesPage2 = _commoditiesPage;
		commoditiesPage2.CategorySelected = (Action<ShopCategory>)Delegate.Combine(commoditiesPage2.CategorySelected, new Action<ShopCategory>(SelectSubCategory));
		ShopCommoditiesPage commoditiesPage3 = _commoditiesPage;
		commoditiesPage3.Selected = (Action<string>)Delegate.Combine(commoditiesPage3.Selected, new Action<string>(SelectCommodity));
		ShopCommoditiesPage commoditiesPage4 = _commoditiesPage;
		commoditiesPage4.CoinTransferClicked = (Action)Delegate.Combine(commoditiesPage4.CoinTransferClicked, (Action)delegate
		{
			TransferCoinPopup transferCoinPopup = UIManager.Popup.Tooltip<TransferCoinPopup>();
			transferCoinPopup.Set();
			transferCoinPopup.Show();
		});
		base.TryClose();
	}

	private ShopCategory GetCurrentCategory()
	{
		return _selectedSubCategory ?? _selectedMainCategory;
	}

	protected override bool TryClose()
	{
		if (_purchasedPage.IsShow)
		{
			HidePurchasedPage();
			return false;
		}
		UIManager.Popup.Tooltip<TransferCoinPopup>().Hide();
		return base.TryClose();
	}

	public override bool Open()
	{
		bool result = base.Open();
		Refresh(reset: true);
		return result;
	}

	public bool Open(ShopCategory category)
	{
		_isPurchasePage = false;
		ShopCategory shopCategory = null;
		ShopCategory shopCategory2 = null;
		HidePurchasedPage();
		int num = ShopCategories.GetCategories().IndexOf(category);
		if (num != -1)
		{
			shopCategory = category;
		}
		else
		{
			ShopCategory[] categories = ShopCategories.GetCategories();
			foreach (ShopCategory shopCategory3 in categories)
			{
				if (shopCategory3.Childs != null && shopCategory3.Childs.IndexOf(category) != -1)
				{
					shopCategory = shopCategory3;
					shopCategory2 = category;
					break;
				}
			}
		}
		if (shopCategory == null && shopCategory2 == null)
		{
			return false;
		}
		_selectedMainCategory = shopCategory;
		_selectedSubCategory = shopCategory2;
		return Open();
	}

	public void Open(string commodityId, bool select)
	{
		Durango.Logic.Shop.Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(commodityId);
		HidePurchasedPage();
		if (commodity == null)
		{
			Open();
			return;
		}
		ShopCategory shopCategory = ShopCategories.FindCategory(Enumerable.Reverse(_tabList.Categories), (ShopCategory cat) => IsValidCommodity(cat, commodity));
		if (shopCategory != null)
		{
			_reservedSelectCommodity = commodityId;
			if (!Open(shopCategory))
			{
				_commoditiesPage.SelectAndMoveTo(commodityId);
			}
		}
		else
		{
			Open();
		}
		if (select)
		{
			SelectCommodity(commodityId);
		}
	}

	[Uri("Purchases")]
	public void OpenPurchases()
	{
		_isPurchasePage = true;
		_selectedMainCategory = null;
		_selectedSubCategory = null;
		HidePurchasedPage();
		if (base.IsOpened)
		{
			Refresh(reset: true);
		}
		else
		{
			Open();
		}
	}

	private void OnOpened()
	{
		_isBuying = false;
		_isCheckingBoughtItems = false;
		_mainWidget.alpha = 0f;
		HidePurchasedPage();
		UIManager.Popup.LoadingRing.AttachToWidget(_mainWidget.gameObject);
		GameSystem<ShopSystem>.Instance().GetPurchasableCommodities(OnPurchasbleCommodities);
		GameSystem<ShopSystem>.Instance().GetPurchases();
	}

	private void OnClosed()
	{
		_buyReservedCommodity = null;
		_reservedSelectCommodity = null;
		_selectedMainCategory = null;
		_selectedSubCategory = null;
		_lastSelectedCategory = null;
		GameSystem<ShopSystem>.Instance().AddReadCommodities(_filteredCommodities);
		_filteredCommodities.Clear();
	}

	private void OnPurchasbleCommodities(List<Durango.Logic.Shop.Commodity> list)
	{
		UIManager.Popup.LoadingRing.DetachFromWidget(_mainWidget.gameObject);
		_mainWidget.alpha = 1f;
		RefreshTabList();
		Refresh(reset: true);
		if (!string.IsNullOrEmpty(_reservedSelectCommodity))
		{
			_commoditiesPage.SelectAndMoveTo(_reservedSelectCommodity);
		}
	}

	public static string ToSubPurchaseKey(string purchaseId, string subId)
	{
		return $"Purchase.{purchaseId}.{subId}";
	}

	private void OnChangeReadCommodities()
	{
		List<Durango.Logic.Shop.Commodity> purchasableList = GameSystem<ShopSystem>.Instance().PurchasableList;
		if (KUtility.GetSize(purchasableList) == 0)
		{
			return;
		}
		RefreshTabList();
		foreach (ShopCategory category in _tabList.Categories)
		{
			CheckHasNewCommodity(category, purchasableList);
		}
		_hasNewCommodityNotification.On = _hasNewCommodityCategories.Count > 0;
		RefreshNotification();
	}

	private void OnUpdateWallet()
	{
		if (_buyReservedCommodity != null)
		{
			Money money = _buyReservedCommodity.Money;
			if (InventorySystem.Wallet.GetBalance(money.Currency) >= money.Amount)
			{
				string id = _buyReservedCommodity.Id;
				_buyReservedCommodity = null;
				Open(id, select: true);
			}
		}
	}

	private bool CheckHasNewCommodity(ShopCategory category, List<Durango.Logic.Shop.Commodity> list)
	{
		bool flag = false;
		if (KUtility.GetSize(category.Childs) > 0)
		{
			ShopCategory[] childs = category.Childs;
			foreach (ShopCategory category2 in childs)
			{
				flag |= CheckHasNewCommodity(category2, list);
			}
		}
		else
		{
			foreach (Durango.Logic.Shop.Commodity item in list)
			{
				if (!IsValidCommodity(category, item) || GameSystem<ShopSystem>.Instance().IsReadCommodity(item.Id))
				{
					continue;
				}
				flag = true;
				break;
			}
		}
		if (flag)
		{
			_hasNewCommodityCategories.Add(category);
		}
		else
		{
			_hasNewCommodityCategories.Remove(category);
		}
		return flag;
	}

	private void OnNewAcceptableSubPurchaseItem(string purchaseId, string commodityId, string subId)
	{
		Durango.Logic.Shop.Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(commodityId);
		if (commodity == null || !commodity.IsQuestPurchase(CommodityCondition.Type.Level))
		{
			return;
		}
		Durango.Logic.Shop.Commodity commodity2 = null;
		int i = 0;
		for (int size = KUtility.GetSize(commodity.SubCommodities); i < size; i++)
		{
			if (commodity.SubCommodities[i].Id == subId)
			{
				commodity2 = commodity.SubCommodities[i];
				break;
			}
		}
		if (commodity2 != null)
		{
			string key = ToSubPurchaseKey(purchaseId, subId);
			UIManager.Alarm.ShowNotify(T._("<em>{0}</em>{0:-을} 달성했습니다. 상점에서 <em>{0}</em>의 보상을 수령해주세요.", commodity2.Title, commodity.Title), "icon_mainhud_shop", major: true, 3600f, delegate
			{
				OpenPurchases();
				ShowSubCommodityStatus(GameSystem<ShopSystem>.Instance().GetPurchase(purchaseId));
			}, key);
		}
	}

	private void OnDomesticationResult(DomesticationResult result)
	{
		string reinId = result.ReinId;
		if (!string.IsNullOrEmpty(reinId))
		{
			GameSystem<InventorySystem>.Instance().AddOnItemEvent(reinId, delegate(ItemData item)
			{
				Reins? reins = item.Reins;
				DomesticationRewardPopup domesticationRewardPopup = UIManager.Popup.Tooltip<DomesticationRewardPopup>();
				domesticationRewardPopup.SetLevel(item.Level).SetType((reins.HasValue && reins.Value.Pet.HasValue) ? reins.Value.Pet.Value.GetAnimalType() : 0).SetConfirm(T._("확인"), null)
					.SetResult(result)
					.Show();
			});
		}
	}

	private void OnAcceptableSubPurchasesUpdated()
	{
		_acceptableSubPurchaseNotifiaction.On = GameSystem<ShopSystem>.Instance().HasAcceptableSubPurchase(CommodityCondition.Type.Level);
		if (!base.IsOpened || _isPurchasePage)
		{
			return;
		}
		bool flag = false;
		List<Durango.Logic.Shop.Commodity> list = FilterCommodities(GetCurrentCategory(), _bufferCommodities);
		for (int i = 0; i < list.Count; i++)
		{
			if (list[i].IsQuestPurchase(CommodityCondition.Type.Level))
			{
				flag = true;
			}
		}
		if (flag)
		{
			Refresh(reset: false);
		}
	}

	private void OnUserFirstPurchaseHistoryUpdated()
	{
		if (base.IsOpened)
		{
			Refresh(reset: false);
		}
	}

	private void OnUpdatePurchases()
	{
		bool flag = false;
		List<Durango.Logic.Shop.Purchase> purchases = GameSystem<ShopSystem>.Instance().Purchases;
		for (int i = 0; i < purchases.Count; i++)
		{
			Durango.Logic.Shop.Purchase purchase = purchases[i];
			if (purchase.HasSubCommodities)
			{
				AcceptableSubPurchase? acceptableSubPurchase = GameSystem<ShopSystem>.Instance().GetAcceptableSubPurchase(purchase.Id);
				if (!acceptableSubPurchase.HasValue || purchase.SubCommodityConditionType != CommodityCondition.Type.Level)
				{
					continue;
				}
				string[] acceptableSubIds = acceptableSubPurchase.Value.AcceptableSubIds;
				foreach (string key in acceptableSubIds)
				{
					if (!purchase.GetSubAcceptedAt(key).HasValue)
					{
						flag = true;
						break;
					}
				}
			}
			else
			{
				flag = true;
			}
			if (flag)
			{
				break;
			}
		}
		_purchsesNotifiaction.On = flag;
		if (base.IsOpened && _isPurchasePage)
		{
			Refresh(reset: false);
		}
	}

	private void Refresh(bool reset)
	{
		if (!_isPurchasePage)
		{
			if (_selectedMainCategory == null)
			{
				_selectedMainCategory = ShopCategories.GetCategories().FirstOrDefault();
			}
			if (_selectedMainCategory != null && _selectedSubCategory == null)
			{
				List<ShopCategory> list = _subCategories.Get(_selectedMainCategory);
				if (list != null && list.Count > 0)
				{
					_selectedSubCategory = list[0];
				}
			}
		}
		ShopCategory currentCategory = GetCurrentCategory();
		if (_lastSelectedCategory != null && _lastSelectedCategory != currentCategory)
		{
			GameSystem<ShopSystem>.Instance().AddReadCommodities(_filteredCommodities);
		}
		_lastSelectedCategory = currentCategory;
		if (!_isPurchasePage && currentCategory.IsShowPromotion())
		{
			_promotionWidget.Set(Yaml.Util.Singleton<Yaml.Commodities>.Instance.PromotionLinks);
		}
		else
		{
			_promotionWidget.gameObject.SetActive(value: false);
		}
		_filteredCommodities.Clear();
		if (_isPurchasePage)
		{
			_tabList.SelectPurchaseTab();
			_purchasesPage.Show(reset);
			_commoditiesPageWidget.gameObject.SetActive(value: false);
		}
		else
		{
			_tabList.SelectCategory(_selectedMainCategory);
			ShopCategory currentCategory2 = GetCurrentCategory();
			FilterCommodities(currentCategory2, _filteredCommodities);
			_commoditiesPage.Set(currentCategory2, _filteredCommodities, reset);
			_commoditiesPage.SetSubCategories(_subCategories.Get(_selectedMainCategory), _selectedSubCategory);
			_commoditiesPageWidget.gameObject.SetActive(value: true);
			_purchasesPage.Hide();
		}
		_mainLayout.UpdateLayout();
		UIUtility.UpdateAnchors(_mainLayout.transform);
	}

	private List<Durango.Logic.Shop.Commodity> FilterCommodities(ShopCategory category, List<Durango.Logic.Shop.Commodity> list = null)
	{
		List<Durango.Logic.Shop.Commodity> purchasableList = GameSystem<ShopSystem>.Instance().PurchasableList;
		if (list == null)
		{
			list = new List<Durango.Logic.Shop.Commodity>();
		}
		list.Clear();
		int i = 0;
		for (int size = KUtility.GetSize(purchasableList); i < size; i++)
		{
			Durango.Logic.Shop.Commodity commodity = purchasableList[i];
			if (IsValidCommodity(category, commodity))
			{
				list.Add(commodity);
			}
		}
		list.Sort();
		return list;
	}

	private static bool IsValidCommodity(ShopCategory category, Durango.Logic.Shop.Commodity commodity)
	{
		return commodity.IsVisible() && category.IsValidCommodity(commodity);
	}

	private void RefreshTabList()
	{
		_subCategories.Clear();
		_tabList.SettingBegin();
		ShopCategory[] categories = ShopCategories.GetCategories();
		foreach (ShopCategory shopCategory in categories)
		{
			if (FilterCommodities(shopCategory, _bufferCommodities).Count == 0)
			{
				continue;
			}
			_tabList.AddTab(shopCategory);
			if (shopCategory.Childs == null)
			{
				continue;
			}
			List<ShopCategory> list = null;
			ShopCategory[] childs = shopCategory.Childs;
			foreach (ShopCategory shopCategory2 in childs)
			{
				if (FilterCommodities(shopCategory2, _bufferCommodities).Count != 0)
				{
					if (list == null)
					{
						list = new List<ShopCategory>();
					}
					list.Add(shopCategory2);
				}
			}
			if (list != null)
			{
				_subCategories.Add(shopCategory, list);
			}
		}
		_tabList.SettingFinish();
		RefreshNotification();
	}

	private void RefreshNotification()
	{
		if (base.IsOpened)
		{
			int i = 0;
			for (int count = _tabList.Categories.Count; i < count; i++)
			{
				ShopCategory cat = _tabList.Categories[i];
				GetCategoryNotifiaction(cat, out var on, out var type);
				_tabList.SetNotification(i, on, type);
			}
			_commoditiesPage.RefreshCategoryNotification();
		}
	}

	public void GetCategoryNotifiaction(ShopCategory cat, out bool on, out Durango.Logic.Notification.Type type)
	{
		on = false;
		type = Durango.Logic.Notification.Type.Normal;
		if (_hasNewCommodityCategories.Contains(cat))
		{
			on = true;
			type = _hasNewCommodityNotification.Type;
		}
	}

	public void ShowPurchasedPage(Durango.Logic.Shop.Commodity commodity, Purchased purchased, bool withVoucher)
	{
		_purchasedPage.Show(commodity, purchased, withVoucher);
		_mainWidget.gameObject.SetActive(value: false);
	}

	public void HidePurchasedPage()
	{
		_purchasedPage.Hide();
		_mainWidget.gameObject.SetActive(value: true);
	}

	private void SelectSubCategory(ShopCategory category)
	{
		_selectedSubCategory = category;
		Refresh(reset: true);
	}

	private void SelectCommodity(string id)
	{
		Durango.Logic.Shop.Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(id);
		if (commodity != null)
		{
			Durango.Logic.Shop.Purchase questPurchase = commodity.GetQuestPurchase(CommodityCondition.Type.Level);
			if (questPurchase != null)
			{
				ShowSubCommodityStatus(questPurchase);
			}
			else
			{
				BuyCommodity(commodity);
			}
		}
	}

	public static void ShowSubCommodityStatus(Durango.Logic.Shop.Purchase purchase)
	{
		if (purchase != null)
		{
			SubCommoditiesPopup subCommoditiesPopup = UIManager.Popup.Tooltip<SubCommoditiesPopup>();
			subCommoditiesPopup.Set(purchase);
			subCommoditiesPopup.Show();
		}
	}

	public void BuyCommodity(Durango.Logic.Shop.Commodity commodity)
	{
		if (_isBuying || _isCheckingBoughtItems)
		{
			return;
		}
		if (commodity.Data.Type == CommodityType.PlayerSlot)
		{
			SetIsBuying(isBuying: true);
			Clusters.RequestAccounts(GameManager.GatewayUrl, delegate(Account account)
			{
				SetIsBuying(isBuying: false);
				if (account != null)
				{
					int size = KUtility.GetSize(account.Players);
					if (size <= account.PlayerSlotCount)
					{
						BuyCommodityCalled(commodity);
					}
					else
					{
						MessageBox messageBox = UIManager.MessageBox;
						messageBox.AddKeyValueInfo(T._("현재 보유한 캐릭터"), $"<alert>{size}</alert>");
						messageBox.AddKeyValueInfo(T._("캐릭터 슬롯 수"), account.PlayerSlotCount.ToString());
						messageBox.Show(T._("캐릭터 슬롯을 구매하시겠습니까?"), T._("[icon=icon_make_alert] 캐릭터 슬롯의 개수보다 캐릭터 개수가 더 많기 때문에, 지금 이상품을 구입하셔도 <alert>즉시 추가 캐릭터를 생성할 수 없습니다.</alert>\n[icon=icon_make_alert] 기존 캐릭터를 삭제하거나, {0}개의 추가 슬롯을 구입하면 새 캐릭터를 생성할 수 있습니다.", size + 1 - account.PlayerSlotCount), delegate(bool ok)
						{
							if (ok)
							{
								BuyCommodityCalled(commodity);
							}
						});
					}
				}
			});
		}
		else
		{
			BuyCommodityCalled(commodity);
		}
	}

	private void BuyCommodityCalled(Durango.Logic.Shop.Commodity commodity)
	{
		ShopBuyConfirmPopup shopBuyConfirmPopup = UIManager.Popup.Tooltip<ShopBuyConfirmPopup>();
		shopBuyConfirmPopup.Set(commodity, OnBuyCommodity);
		shopBuyConfirmPopup.Show();
	}

	private void OnBuyCommodity(Durango.Logic.Shop.Commodity commodity)
	{
		SetIsBuying(isBuying: true);
		string title = commodity.Title;
		GameSystem<ShopSystem>.Instance().PurchaseCommodity(commodity, delegate(Purchased result, bool withVoucher)
		{
			SetIsBuying(isBuying: false);
			SoundManager.PlayEvent("ui_menu_store_item_buy_success");
			switch (commodity.Data.Type)
			{
			case CommodityType.RandomItemBox:
			case CommodityType.RandomEmotionBox:
			case CommodityType.RandomBoxBundle:
			{
				RandomBoxScene.BoxType boxType = ((result.Purchases.Length >= 10) ? RandomBoxScene.BoxType.X10 : RandomBoxScene.BoxType.X1);
				RandomBoxScene.Load(delegate
				{
					ShowPurchasedPage(commodity, result, withVoucher);
				}, boxType);
				break;
			}
			default:
			{
				_isCheckingBoughtItems = true;
				ShopBoughtPopup shopBoughtPopup = UIManager.Popup.Tooltip<ShopBoughtPopup>();
				shopBoughtPopup.Set(commodity, title);
				shopBoughtPopup.Show();
				shopBoughtPopup.AddOnFinished(OnBoughtConfirmFinished);
				break;
			}
			}
			GameSystem<ShopSystem>.Instance().GetPurchasableCommodities(delegate
			{
				Refresh(reset: false);
			}, immediately: true);
			if (SingletonDict<string, SpecialDealBanner>.Instance.ContainsKey(commodity.Id))
			{
				GameSystem<ShopSystem>.Instance().GetSpecialDeals();
			}
		}, delegate
		{
			SetIsBuying(isBuying: false);
			SoundManager.PlayEvent("ui_menu_store_item_buy_fail");
			Money money = commodity.Money;
			if ((!commodity.CommodityInfo.MaxPurchasableCount.HasValue || commodity.CommodityInfo.MaxPurchasableCount.Value > 0) && InventorySystem.Wallet.GetBalance(money.Currency) < money.Amount)
			{
				ShowCommdityLackCurrency(commodity);
			}
		});
	}

	private void OnBoughtConfirmFinished()
	{
		_isCheckingBoughtItems = false;
	}

	private void SetIsBuying(bool isBuying)
	{
		_isBuying = isBuying;
		UIManager.ShowLoadingIcon(isBuying);
	}

	public void ShowCommdityLackCurrency(Durango.Logic.Shop.Commodity commodity)
	{
		Money money = commodity.Money;
		long num = money.Amount - InventorySystem.Wallet.GetBalance(money.Currency);
		List<Durango.Logic.Shop.Commodity> purchasableList = GameSystem<ShopSystem>.Instance().PurchasableList;
		Durango.Logic.Shop.Commodity commodity2 = null;
		long num2 = long.MaxValue;
		foreach (Durango.Logic.Shop.Commodity item in purchasableList)
		{
			if (!item.IsVisible())
			{
				continue;
			}
			Currency currency = money.Currency;
			long num3;
			if (currency != Currency.Gem)
			{
				if (currency != Currency.Coin)
				{
					continue;
				}
				num3 = item.CoinAmount;
			}
			else
			{
				num3 = item.Data.GemAmount;
			}
			if (num3 >= num)
			{
				long num4 = num3 - num;
				if (num4 < num2)
				{
					num2 = num4;
					commodity2 = item;
				}
			}
		}
		if (commodity2 != null)
		{
			_buyReservedCommodity = commodity;
			Open(commodity2.Id, select: true);
		}
	}

	private void UpdatePurchaseNotification()
	{
		_tabList.PurchaseTab.SetNotifiation(_purchsesNotifiaction.On);
	}

	private void UpdateCurrencyWidgets(IEnumerable<Durango.Logic.Shop.Commodity> commodity)
	{
		_currencyWidgets.BeginLoad();
		using (Reusable<HashSet<Currency>> reusable = ReusableHashSet<Currency>.Pop())
		{
			using Reusable<HashSet<string>> reusable2 = ReusableHashSet<string>.Pop();
			Currency currencyType = Currency.TStone;
			foreach (Durango.Logic.Shop.Commodity item in commodity)
			{
				switch (item.Money.Currency)
				{
				case Currency.TStone:
				case Currency.Gem:
				case Currency.Coin:
					currencyType = item.Data.PriceCurrency;
					break;
				default:
					reusable.Value.Add(item.Data.PriceCurrency);
					break;
				}
				if (!string.IsNullOrEmpty(item.Data.VoucherId))
				{
					reusable2.Value.Add(item.Data.VoucherId);
				}
			}
			if (reusable.Value.Count > 0 || reusable2.Value.Count > 0)
			{
				_currencyWidgets.GetNext().SetCurrencyType(currencyType);
				foreach (Currency item2 in reusable.Value)
				{
					_currencyWidgets.GetNext().SetCurrencyType(item2);
				}
				foreach (string item3 in reusable2.Value)
				{
					_currencyWidgets.GetNext().SetVoucherType(item3);
				}
			}
			else
			{
				_currencyWidgets.GetNext().SetCurrencyType(Currency.TStone);
				_currencyWidgets.GetNext().SetCurrencyType(Currency.Gem);
				_currencyWidgets.GetNext().SetCurrencyType(Currency.Coin);
			}
		}
		_currencyWidgets.EndLoad();
		_currencyWidgets.Reposition(Vector3.left, 5);
	}

	[Uri("Category")]
	private void Open(string key)
	{
		if (!string.IsNullOrEmpty(key))
		{
			ShopCategory shopCategory = ShopCategories.FindCategory(key);
			if (shopCategory != null)
			{
				Open(shopCategory);
			}
		}
	}

	[Uri("Commodity")]
	private void CommodityUri(string id)
	{
		Open(id, select: true);
	}
}
