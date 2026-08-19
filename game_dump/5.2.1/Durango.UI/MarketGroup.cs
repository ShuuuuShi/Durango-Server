using System;
using System.Collections.Generic;
using Durango.Logic.Explore;
using Durango.Logic.Item;
using Durango.Logic.Market;
using Durango.Logic.Notification;
using Durango.Offline;
using Durango.System;
using Durango.UI.Control;
using Durango.UI.Popup;
using Durango.Utils;
using Durango.Utils.Extensions;
using L10N;
using Messages;
using NestedPrefab;
using Shared.Economy;
using Shared.Market;
using UnityEngine;

namespace Durango.UI;

[Uri("Market")]
public class MarketGroup : UIBase, INotificationable
{
	public enum Menu
	{
		[T.EnumName("구입")]
		Buy,
		[T.EnumName("내 장터")]
		MyMarket,
		[T.EnumName("판매")]
		Sell,
		[T.EnumName("테스트")]
		Test
	}

	[SerializeField]
	private UITitle _titleWidget;

	[SerializeField]
	private NestedPrefabLinker _menuLinker;

	[SerializeField]
	private CommodityListWidget _goodsList;

	[SerializeField]
	private MarketHistoryWidget _historyList;

	[SerializeField]
	private SellItemWidget _sellItemWidget;

	private readonly Toggle _notification = new Toggle(Durango.Logic.Notification.Type.Important);

	private IconTabList _menuList;

	private Menu[] _menus;

	private Menu _selectedTab;

	private bool _onExpiredNotification;

	private bool _onPaymentPendingNotification;

	private StackableAlarm<string, ProductSold> _productSoldAlarm;

	[SerializeField]
	private SellItemWidget _CustomWidget;

	public Notification Notification => _notification;

	private void Start()
	{
		_openCloseSound = UISound.GroupType.Default;
		_menuList = _menuLinker.Object.GetComponent<IconTabList>();
		_menus = Enums<Menu>.All();
		_menuList.BeginLoad();
		for (int i = 0; i < _menus.Length; i++)
		{
			Menu menu = _menus[i];
			_menuList.Add(IconMap.Get(menu, "icon_popup_player_note"), menu.GetName());
		}
		_menuList.EndLoad();
		_menuList.Clicked += MenuSelected;
		Observable<bool> hasExpired = GameSystem<MarketSystem>.Instance().HasExpired;
		hasExpired.Changed = (Action<bool>)Delegate.Combine(hasExpired.Changed, (Action<bool>)delegate(bool value)
		{
			_onExpiredNotification = value;
			UpdateNotifiactionMarkers();
		});
		GameSystem<MarketSystem>.Instance().OnProductCollectiblePaymentExists += OnProductCollectiblePaymentExists;
		GameSystem<MarketSystem>.Instance().OnProductSold += OnProductSold;
		GameSystem<MarketSystem>.Instance().OnProductPaymentReceived += OnProductPaymentReceived;
		GameSystem<MarketSystem>.Instance().OnProductStateUpdated += OnProductStateUpdated;
		_productSoldAlarm = new StackableAlarm<string, ProductSold>("ProductSold", (ProductSold sold) => sold.Item.Id, (ProductSold sold, int count) => (count <= 1) ? T._("<em>{0}</em> 아이템이 팔렸습니다.", sold.Item.Name) : T._("<em>{0}</em> 외 {1}개 아이템이 팔렸습니다.", sold.Item.Name, count - 1), "alarm_market", majorAlarm: true, 1.8f, delegate
		{
			if (!base.IsOpened)
			{
				Open();
			}
			SelectMenuTab(Menu.MyMarket);
		});
		SetChildrenActive(activated: false);
	}

	public void OpenAndSearch(OrTagFilter tagFilter, OrTagFilter material, int level = 0)
	{
		Open();
		_goodsList.Open(tagFilter, material, level, instant: true);
	}

	public void OpenAndSearch(string prototype)
	{
		Open();
		_goodsList.Open(prototype, instant: true);
	}

	public void OpenAndSearch(string prototype, int prototypeLevel, string itemTag)
	{
		Open();
		_goodsList.Open(prototype, prototypeLevel, itemTag, instant: true);
	}

	private void ShowMainPage(bool reset)
	{
		if (reset)
		{
			if (!OptionSystem.IsMarketEnabled())
			{
				UIManager.SystemMsg(T._("현재 섬 장터를 점검하고 있습니다. 이용에 불편을 드려 죄송합니다."));
			}
			_goodsList.Open(instant: true);
			_historyList.Close(instant: true);
			_sellItemWidget.Close(instant: true);
			_menuList.Select(_menus.IndexOf(Menu.Buy));
			_selectedTab = Menu.Buy;
		}
		Durango.Logic.Explore.Region region = GameManager.Region;
		_titleWidget.Object.SetTitle(T._("{0} 섬", region.Name));
	}

	protected override bool TryOpen()
	{
		if (!base.TryOpen())
		{
			return false;
		}
		ShowMainPage(reset: true);
		_onExpiredNotification = GameSystem<MarketSystem>.Instance().HasExpired.Value;
		_onPaymentPendingNotification = GameSystem<MarketSystem>.Instance().HasCollectiblePayment;
		UpdateNotifiactionMarkers();
		return true;
	}

	protected override bool TryClose()
	{
		if (!_goodsList.Back())
		{
			return false;
		}
		base.TryClose();
		return true;
	}

	private void OpenGoodsList()
	{
		_goodsList.Open(instant: false);
		_historyList.Close();
		_sellItemWidget.Close();
	}

	private void OpenHistoryList(ProductType type)
	{
		_goodsList.Close();
		_historyList.Open(type);
		_sellItemWidget.Close();
	}

	private void OpenSellItem()
	{
		_goodsList.Close();
		_historyList.Close();
		_sellItemWidget.Open();
	}

	private void MenuSelected(int index)
	{
		Menu menu = _menus[index];
		SelectMenuTab(menu);
	}

	private void SelectMenuTab(Menu menu)
	{
		bool value = false;
		switch (menu)
		{
		case Menu.Buy:
			if (_selectedTab == Menu.Buy)
			{
				ShowMainPage(reset: true);
				break;
			}
			_menuList.Select(_menus.IndexOf(Menu.Buy));
			OpenGoodsList();
			value = true;
			break;
		case Menu.MyMarket:
			_menuList.Select(_menus.IndexOf(Menu.MyMarket));
			OpenHistoryList(ProductType.Sold);
			break;
		case Menu.Sell:
			_menuList.Select(_menus.IndexOf(Menu.Sell));
			OpenSellItem();
			break;
		case Menu.Test:
			_menuList.Select(_menus.IndexOf(Menu.Test));
			OnSelectTestTab();
			break;
		default:
			throw new ArgumentOutOfRangeException();
		}
		_selectedTab = menu;
		HasBack.Value = value;
	}

	private void OnProductCollectiblePaymentExists(bool hasCollectablePayment)
	{
		_onPaymentPendingNotification = hasCollectablePayment;
		UpdateNotifiactionMarkers();
		if (base.IsOpened && _historyList.IsOpenedInSoldTab)
		{
			_historyList.RefreshReceiveButtonBar();
		}
	}

	private void OnProductSold(ProductSold sold)
	{
		_productSoldAlarm.Add(sold);
		string chatText = T._("<em>{0}</em> 아이템이 <em>{1}</em>에 팔렸습니다.", sold.Item.Name, Durango.Logic.Item.Inventory.CurrencyFormat(sold.Price, Currency.TStone));
		GameSystem<SocialSystem>.Instance().AddSystemChat(chatText, string.Empty, remainColor: true);
	}

	private void OnProductPaymentReceived(MarketPaymentReceived received)
	{
		string text = ((received.ItemCount <= 1) ? T._("(수수료 제외) <em>{0}</em> 아이템을 팔아 <em>{1}</em>을 받았습니다.", received.FirstItemName, Durango.Logic.Item.Inventory.CurrencyFormat(received.TotalPrice, Currency.TStone)) : T._("(수수료 제외) <em>{0}</em> 외 {1}개의 아이템을 팔아 <em>{2}</em>을 받았습니다.", received.FirstItemName, received.ItemCount - 1, Durango.Logic.Item.Inventory.CurrencyFormat(received.TotalPrice, Currency.TStone)));
		UIManager.SystemMsg(text);
		GameSystem<SocialSystem>.Instance().AddSystemChat(text, string.Empty, remainColor: true);
	}

	private void OnProductStateUpdated(ProductStateUpdated updated)
	{
		if (updated.State == ProductState.PaymentReceived && base.IsOpened && _historyList.IsOpenedInSoldTab)
		{
			_historyList.PaymentReceived(updated.ProductId);
		}
	}

	private void UpdateNotifiactionMarkers()
	{
		_notification.On = _onExpiredNotification || _onPaymentPendingNotification;
		_menuList.SetNotification(_menus.IndexOf(Menu.MyMarket), _notification.On, _notification.Type);
		_historyList.SetNotification(ProductType.Expired, _onExpiredNotification, _notification.Type);
		_historyList.SetNotification(ProductType.Sold, _onPaymentPendingNotification, _notification.Type);
	}

	private void OnSelectTestTab()
	{
		_goodsList.Close(instant: true);
		_historyList.Close(instant: true);
		_sellItemWidget.Close(instant: true);
		UIManager.MessageBox.Show(T._("더 많은 기능들"), "<alert_icon/> " + ObjectManager.GetTestTitleText(), delegate(int index)
		{
			switch (index)
			{
			case 0:
			{
				GenericSelector genericSelector3 = UIManager.Popup.Tooltip<GenericSelector>();
				genericSelector3.ResetArguments();
				genericSelector3.SetTitle("플레이어 설정");
				genericSelector3.AddItem("HP");
				genericSelector3.AddItem("스태미나");
				genericSelector3.AddItem("피로도");
				genericSelector3.AddItem("티스톤");
				genericSelector3.AddItem("젬");
				genericSelector3.AddItem("듀랑고 코인");
				genericSelector3.SetSelected(delegate(int index5)
				{
					switch (index5)
					{
					case 0:
						UIManager.Popup.Tooltip<TextInputPopup>().Show(TextInHealth, T._("수치를 입력해주세요."), null, isMultiline: true, null, 0);
						break;
					case 1:
						UIManager.Popup.Tooltip<TextInputPopup>().Show(TextInStamina, T._("수치를 입력해주세요."), null, isMultiline: true, null, 0);
						break;
					case 2:
						UIManager.Popup.Tooltip<TextInputPopup>().Show(TextInFatigue, T._("수치를 입력해주세요."), null, isMultiline: true, null, 0);
						break;
					case 3:
						UIManager.Popup.Tooltip<TextInputPopup>().Show(TextInTStone, T._("수치를 입력해주세요."), null, isMultiline: true, null, 0);
						break;
					case 4:
						UIManager.Popup.Tooltip<TextInputPopup>().Show(TextInGem, T._("수치를 입력해주세요."), null, isMultiline: true, null, 0);
						break;
					case 5:
						UIManager.Popup.Tooltip<TextInputPopup>().Show(TextInCoin, T._("수치를 입력해주세요."), null, isMultiline: true, null, 0);
						break;
					}
				});
				genericSelector3.Show();
				break;
			}
			case 1:
			{
				GenericSelector genericSelector2 = UIManager.Popup.Tooltip<GenericSelector>();
				genericSelector2.ResetArguments();
				genericSelector2.SetTitle("야생동물 소환");
				genericSelector2.AddItem("티라노사우루스");
				genericSelector2.AddItem("알로사우루스");
				genericSelector2.AddItem("타르보사우루스");
				genericSelector2.SetSelected(delegate(int index6)
				{
					switch (index6)
					{
					case 0:
						Singleton<AnimalManager>.Instance().MakeAnimal(2005, WildAnimalAI.Type.TRex);
						break;
					case 1:
						Singleton<AnimalManager>.Instance().MakeAnimal(2021, WildAnimalAI.Type.Allo);
						break;
					case 2:
						Singleton<AnimalManager>.Instance().MakeAnimal(2089, WildAnimalAI.Type.Tarbo);
						break;
					}
				});
				genericSelector2.Show();
				break;
			}
			case 2:
			{
				GenericSelector genericSelector = UIManager.Popup.Tooltip<GenericSelector>();
				genericSelector.ResetArguments();
				genericSelector.SetTitle("기타 기능 설정");
				genericSelector.AddItem("서버에 메세지 띄우기");
				genericSelector.AddItem("프롤로그 영상 다시보기");
				genericSelector.AddItem("링크로 동영상 재생");
				genericSelector.SetSelected(delegate(int index5)
				{
					switch (index5)
					{
					case 0:
						UIManager.Popup.Tooltip<TextInputPopup>().Show(TextInSystemMsg, T._("서버에 전달할 메세지를 입력해주세요."), null, isMultiline: true, null, 0);
						break;
					case 1:
						FullScreenMovieGroupBase.Play(Platform_PC.PrologueMovieUrl_PC);
						break;
					case 2:
						UIManager.Popup.Tooltip<TextInputPopup>().Show(TextInMp4URL, T._("재생할 동영상의 주소를 입력해주세요. (유튜브 X)"), null, isMultiline: true, null, 0);
						break;
					}
				});
				genericSelector.Show();
				break;
			}
			}
		}, new MessageBox.Button(T._("플레이어 설정")), new MessageBox.Button(T._("야생동물 소환")), new MessageBox.Button(T._("기타 기능 설정")), T._("취소"));
	}

	private void TextInSystemMsg(string value)
	{
		UIManager.SystemMsg(T._(value));
	}

	private void TextInMp4URL(string value)
	{
		FullScreenMovieGroupBase.Play(value);
	}

	private void TextInHealth(string value)
	{
		if (float.TryParse(value, out var result))
		{
			Gauge life = new Gauge(PlayerBehavior.LocalPlayer.Life.Max(), PlayerBehavior.LocalPlayer.Life.Min(), new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = result
				}
			});
			Durango.Offline.Player.Instance._context.AppearPlayer.Survival.Life = life;
			PlayerBehavior.LocalPlayer.SetSurvivalGauge(Durango.Offline.Player.Instance._context.AppearPlayer.Survival.Life, Durango.Offline.Player.Instance._context.AppearPlayer.Survival.Gauges);
			Durango.Offline.Player.Instance._context.Save();
		}
	}

	private void TextInStamina(string value)
	{
		if (float.TryParse(value, out var result))
		{
			Gauge value2 = new Gauge(PlayerBehavior.LocalPlayer.Stamina.Max(), PlayerBehavior.LocalPlayer.Stamina.Min(), new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = result
				}
			});
			Durango.Offline.Player.Instance._context.AppearPlayer.Survival.Gauges["stamina"] = value2;
			PlayerBehavior.LocalPlayer.SetSurvivalGauge(Durango.Offline.Player.Instance._context.AppearPlayer.Survival.Life, Durango.Offline.Player.Instance._context.AppearPlayer.Survival.Gauges);
			Durango.Offline.Player.Instance._context.Save();
		}
	}

	private void TextInFatigue(string value)
	{
		if (float.TryParse(value, out var result))
		{
			Gauge value2 = new Gauge(PlayerBehavior.LocalPlayer.Fatigue.Max(), PlayerBehavior.LocalPlayer.Fatigue.Min(), new GaugeNode[1]
			{
				new GaugeNode
				{
					Time = 0.0,
					Value = result
				}
			});
			Durango.Offline.Player.Instance._context.AppearPlayer.Survival.Gauges["fatigue"] = value2;
			PlayerBehavior.LocalPlayer.SetSurvivalGauge(Durango.Offline.Player.Instance._context.AppearPlayer.Survival.Life, Durango.Offline.Player.Instance._context.AppearPlayer.Survival.Gauges);
			Durango.Offline.Player.Instance._context.Save();
		}
	}

	private void TextInTStone(string value)
	{
		if (long.TryParse(value, out var result))
		{
			Dictionary<Currency, long> dictionary = new Dictionary<Currency, long>();
			dictionary.Add(Currency.TStone, result);
			Durango.Offline.Player.Instance.SendWalletUpdated(dictionary);
		}
	}

	private void TextInGem(string value)
	{
		if (long.TryParse(value, out var result))
		{
			Dictionary<Currency, long> dictionary = new Dictionary<Currency, long>();
			dictionary.Add(Currency.Gem, result);
			Durango.Offline.Player.Instance.SendWalletUpdated(dictionary);
		}
	}

	private void TextInCoin(string value)
	{
		if (long.TryParse(value, out var result))
		{
			Dictionary<Currency, long> dictionary = new Dictionary<Currency, long>();
			dictionary.Add(Currency.Coin, result);
			Durango.Offline.Player.Instance.SendWalletUpdated(dictionary);
		}
	}
}
