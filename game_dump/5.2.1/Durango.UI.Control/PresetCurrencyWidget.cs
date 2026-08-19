using System;
using System.Collections.Generic;
using System.Linq;
using Durango.Logic;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.System;
using Durango.UI.Popup;
using L10N;
using Messages;
using Shared.Economy;
using Shared.Purchaser;
using Shared.Season2;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI.Control;

public class PresetCurrencyWidget : UIWidget
{
	private enum EventType
	{
		Currency,
		Voucher,
		SkillPoint,
		WarpRush
	}

	public Action LayoutUpdated;

	protected bool _isInit;

	[SerializeField]
	private UISprite _iconSprite;

	[SerializeField]
	private UILabel _amountLabel;

	[SerializeField]
	private GameObject _extraButton;

	[SerializeField]
	private GameObject _chargeButton;

	[SerializeField]
	private GameObject _infoButton;

	[SerializeField]
	private RectLayout _layout;

	private Currency _currencyType = Currency.Invalid;

	private string _voucherId;

	private bool _clanFund;

	private bool _skillPoint;

	private bool _isEnabled;

	private EventType? _eventType;

	private ResourceType _warpRushStoneType = ResourceType.Invalid;

	private bool _isTotalWarpRushStone;

	protected bool IsButtonActive
	{
		get
		{
			if (!_chargeButton.activeSelf)
			{
				return _infoButton.activeSelf;
			}
			return true;
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		if (Application.isPlaying)
		{
			_isEnabled = true;
			AddEvent();
			Refresh();
		}
	}

	protected override void OnDisable()
	{
		base.OnDisable();
		if (Application.isPlaying)
		{
			ClearEvent();
			_isEnabled = false;
		}
	}

	private void AddEvent()
	{
		EventType? eventType = ((_currencyType != Currency.Invalid) ? new EventType?(EventType.Currency) : ((!string.IsNullOrEmpty(_voucherId)) ? new EventType?(EventType.Voucher) : (_skillPoint ? new EventType?(EventType.SkillPoint) : ((_warpRushStoneType == ResourceType.Invalid) ? null : new EventType?(EventType.WarpRush)))));
		if (_eventType == eventType)
		{
			return;
		}
		ClearEvent();
		_eventType = eventType;
		EventType? eventType2 = _eventType;
		if (!eventType2.HasValue)
		{
			return;
		}
		switch (_eventType.Value)
		{
		case EventType.Currency:
		case EventType.Voucher:
			GameSystem<InventorySystem>.Instance().WalletUpdated += OnUpdateWallet;
			break;
		case EventType.SkillPoint:
			GameSystem<SkillSystem>.Instance().SkillListUpdated += OnUpdateSkills;
			break;
		case EventType.WarpRush:
			if (_isTotalWarpRushStone)
			{
				GameSystem<WarpRushSystem>.Instance().TotalResourcesUpdated += WarpRushSystem_RegionResourceUpdated;
			}
			else
			{
				GameSystem<WarpRushSystem>.Instance().RegionResourceUpdated += WarpRushSystem_RegionResourceUpdated;
			}
			break;
		}
	}

	private void ClearEvent()
	{
		EventType? eventType = _eventType;
		if (!eventType.HasValue)
		{
			return;
		}
		switch (_eventType.Value)
		{
		case EventType.Currency:
		case EventType.Voucher:
			GameSystem<InventorySystem>.Instance().WalletUpdated -= OnUpdateWallet;
			break;
		case EventType.SkillPoint:
			GameSystem<SkillSystem>.Instance().SkillListUpdated -= OnUpdateSkills;
			break;
		case EventType.WarpRush:
			if (_isTotalWarpRushStone)
			{
				GameSystem<WarpRushSystem>.Instance().TotalResourcesUpdated -= WarpRushSystem_RegionResourceUpdated;
			}
			else
			{
				GameSystem<WarpRushSystem>.Instance().RegionResourceUpdated -= WarpRushSystem_RegionResourceUpdated;
			}
			break;
		}
		_eventType = null;
	}

	private void Refresh()
	{
		if (!_isInit)
		{
			return;
		}
		if (_currencyType != Currency.Invalid)
		{
			OnUpdateWallet();
		}
		else if (!string.IsNullOrEmpty(_voucherId))
		{
			OnUpdateWallet();
		}
		else if (_clanFund)
		{
			ClanSystem.GetClanFund(delegate(Costs costs)
			{
				_amountLabel.text = Durango.Logic.Item.Inventory.CurrencyFormat(costs._Costs.Get(Currency.TStone, 0L));
			});
		}
		else if (_skillPoint)
		{
			OnUpdateSkills();
		}
		else if (_warpRushStoneType != ResourceType.Invalid)
		{
			WarpRushSystem_RegionResourceUpdated();
		}
	}

	private void OnClick()
	{
		if (_extraButton.gameObject.activeSelf)
		{
			if (_chargeButton.gameObject.activeSelf)
			{
				ChargeCurrency(_currencyType);
			}
			if (_infoButton.gameObject.activeSelf)
			{
				ShowTooltip();
			}
		}
	}

	protected virtual void OnUpdateWallet()
	{
		if (_currencyType != Currency.Invalid)
		{
			UIManager.SystemMsg("test");
			_amountLabel.text = Durango.Logic.Item.Inventory.CurrencyFormat(InventorySystem.Wallet.GetBalance(_currencyType));
		}
		else if (!string.IsNullOrEmpty(_voucherId))
		{
			int voucherCount = InventorySystem.Wallet.GetVoucherCount(_voucherId);
			Voucher voucher = SingletonDict<string, Voucher>.Get(_voucherId);
			_amountLabel.text = ((voucher.CountMax <= 0) ? voucherCount.ToString() : $"{voucherCount} <weak>/ {voucher.CountMax}</weak>");
		}
		UpdateLayout();
	}

	private void OnUpdateSkills()
	{
		_amountLabel.text = $"<em>{GameSystem<SkillSystem>.Instance().RemainSkillPoint}</em> <weak>/ {GameSystem<SkillSystem>.Instance().SkillPoint}</weak>";
		UpdateLayout();
	}

	public static bool IsChargable(Currency currency)
	{
		bool flag;
		switch (currency.Normalize())
		{
		case Currency.Gem:
		case Currency.RPiece:
			flag = true;
			break;
		case Currency.MobileCoin:
			flag = !Platform.Instance.UsePCCoin;
			break;
		case Currency.PcCoin:
			flag = Platform.Instance.UsePCCoin;
			break;
		default:
			return false;
		}
		if (flag)
		{
			return IsShopEnabled();
		}
		return false;
	}

	private static bool IsShopEnabled()
	{
		if (GameSystem<ShopSystem>.Instance().PurchasableList != null)
		{
			return GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Shop);
		}
		return false;
	}

	public static void ChargeCurrency(Currency currency)
	{
		if (!IsChargable(currency))
		{
			return;
		}
		GameSystem<ShopSystem>.Instance().GetPurchasableCommodities(delegate(List<Durango.Logic.Shop.Commodity> list)
		{
			CommodityType type;
			switch (currency.Normalize())
			{
			default:
				return;
			case Currency.Gem:
				type = CommodityType.Gem;
				break;
			case Currency.RPiece:
				type = CommodityType.RPiece;
				break;
			case Currency.MobileCoin:
			case Currency.PcCoin:
				type = CommodityType.Coin;
				break;
			case Currency.Coin:
			case Currency.CashshopMileage:
				return;
			}
			Durango.Logic.Shop.Commodity commodity = null;
			bool flag = true;
			foreach (Durango.Logic.Shop.Commodity item in list)
			{
				if (item.Data.Type == type)
				{
					flag = false;
					if ((item.SalesTag & Shared.Purchaser.Tags.Representative) != 0)
					{
						commodity = item;
						break;
					}
				}
			}
			ShopCategory shopCategory = null;
			if (commodity == null)
			{
				shopCategory = ShopCategories.FindCategory((ShopCategory category) => category.Conditions != null && category.Conditions.Any((ShopCategoryCondition cond) => cond.Type.HasValue && cond.Type.Value == type));
				if (shopCategory == null || flag)
				{
					UIManager.SystemMsg(T._("구매할 상품이 존재하지 않습니다. 다시 확인해 주세요."));
					return;
				}
			}
			UIManager.MessageBox.Hide();
			TooltipBase.CloseAll();
			UISound.PlayClick(UISound.ClickType.ButtonDefault);
			if (commodity != null)
			{
				UIManager.FindScript<ShopGroup>().Open(commodity.Id, select: true);
			}
			else
			{
				UIManager.FindScript<ShopGroup>().Open(shopCategory);
			}
		});
	}

	public virtual void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			UIWidget componentInParent = base.transform.parent.GetComponentInParent<UIWidget>();
			GetComponent<UIWidget>().SetAnchor(componentInParent.gameObject, 0, 0, 0, 0);
			int num = componentInParent.depth;
			UIWidget[] componentsInChildren = base.gameObject.GetComponentsInChildren<UIWidget>(includeInactive: true);
			for (int i = 0; i < componentsInChildren.Length; i++)
			{
				componentsInChildren[i].depth += num;
			}
		}
	}

	private void ResetCurrency()
	{
		_currencyType = Currency.Invalid;
		_voucherId = null;
		_clanFund = false;
		_skillPoint = false;
		_warpRushStoneType = ResourceType.Invalid;
		_chargeButton.SetActive(value: false);
		_infoButton.SetActive(value: false);
	}

	public void SetCurrencyType(Currency type)
	{
		if (type == _currencyType && _isEnabled)
		{
			Refresh();
			return;
		}
		ResetCurrency();
		_currencyType = type;
		_iconSprite.spriteName = Durango.Logic.Item.Inventory.GetIcon(type);
		_iconSprite.color = Color.white;
		switch (type.Normalize())
		{
		case Currency.TStone:
			UIManager.SystemMsg("test");
			break;
		case Currency.Gem:
			UIManager.SystemMsg("test");
			break;
		case Currency.MobileCoin:
		case Currency.PcCoin:
			_chargeButton.SetActive(GameSystem<MenuSystem>.Instance().IsEnabled(MenuType.Shop));
			break;
		case Currency.RPiece:
			_chargeButton.SetActive(value: true);
			break;
		}
		UpdateLayout();
		if (_isEnabled)
		{
			AddEvent();
			Refresh();
		}
	}

	public void SetVoucherType(string voucherId)
	{
		if (voucherId == _voucherId && _isEnabled)
		{
			Refresh();
			return;
		}
		ResetCurrency();
		_voucherId = voucherId;
		Voucher voucher = SingletonDict<string, Voucher>.Get(voucherId);
		_iconSprite.spriteName = voucher.Icon;
		_iconSprite.color = NGUIText.ParseColor(voucher.GetHexColor());
		UpdateLayout();
		if (_isEnabled)
		{
			AddEvent();
			Refresh();
		}
	}

	public void SetClanFund()
	{
		if (_clanFund && _isEnabled)
		{
			Refresh();
			return;
		}
		ResetCurrency();
		_clanFund = true;
		_iconSprite.spriteName = "tstone_icon_clan";
		_iconSprite.color = Color.white;
		_amountLabel.text = string.Empty;
		UpdateLayout();
		if (_isEnabled)
		{
			AddEvent();
			Refresh();
		}
	}

	public void SetSkillPoint()
	{
		if (_skillPoint && _isEnabled)
		{
			Refresh();
			return;
		}
		ResetCurrency();
		_skillPoint = true;
		_iconSprite.spriteName = "icon_sp";
		_iconSprite.color = Color.white;
		_amountLabel.text = string.Empty;
		UpdateLayout();
		if (_isEnabled)
		{
			AddEvent();
			Refresh();
		}
	}

	public void SetWarpRushResource(ResourceType stoneType, bool total)
	{
		if (_warpRushStoneType == stoneType && _isTotalWarpRushStone == total && _isEnabled)
		{
			Refresh();
			return;
		}
		ResetCurrency();
		_warpRushStoneType = stoneType;
		_isTotalWarpRushStone = total;
		switch (_warpRushStoneType)
		{
		case ResourceType.AlphaStone:
			_iconSprite.spriteName = "material_s02_alpha";
			break;
		case ResourceType.BravoStone:
			_iconSprite.spriteName = "material_s02_bravo";
			break;
		case ResourceType.CharlieStone:
			_iconSprite.spriteName = "material_s02_charlie";
			break;
		}
		_iconSprite.color = Color.white;
		_infoButton.SetActive(value: true);
		UpdateLayout();
		if (_isEnabled)
		{
			Refresh();
			AddEvent();
		}
	}

	public void HideExtraButton(bool hide)
	{
		if (!_extraButton.gameObject.activeSelf != hide)
		{
			_extraButton.gameObject.SetActive(!hide);
			UpdateLayout();
		}
	}

	protected virtual void UpdateLayout()
	{
		_layout.UpdateLayout();
		if (LayoutUpdated != null)
		{
			LayoutUpdated();
		}
	}

	private void ShowTooltip()
	{
		if (_warpRushStoneType != ResourceType.Invalid)
		{
			string text = null;
			switch (_warpRushStoneType)
			{
			case ResourceType.CharlieStone:
				text = T._("<em>찰리 스톤</em> 보유량");
				break;
			case ResourceType.BravoStone:
				text = T._("<em>브라보 스톤</em> 보유량");
				break;
			case ResourceType.AlphaStone:
				text = T._("<em>알파 스톤</em> 보유량");
				break;
			}
			WidgetTooltipControl widgetTooltipControl = UIManager.Popup.Tooltip<WidgetTooltipControl>();
			widgetTooltipControl.Set(null, text);
			widgetTooltipControl.Sign = -1;
			widgetTooltipControl.Direction = TooltipBase.TooltipDirection.Vertical;
			widgetTooltipControl.Show(10f);
		}
	}

	private void WarpRushSystem_RegionResourceUpdated()
	{
		int num = ((!_isTotalWarpRushStone) ? GameSystem<WarpRushSystem>.Instance().GetWarpRushRegionResource(_warpRushStoneType) : GameSystem<WarpRushSystem>.Instance().GetWarpRushTotalResource(_warpRushStoneType));
		_amountLabel.text = num.ToString();
	}
}
