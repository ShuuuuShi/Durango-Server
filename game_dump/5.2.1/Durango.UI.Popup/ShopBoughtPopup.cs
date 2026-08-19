using System;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.UI.Control;
using L10N;
using Shared.Economy;
using Shared.Purchaser;
using UnityEngine;
using Yaml;

namespace Durango.UI.Popup;

public class ShopBoughtPopup : TooltipBase
{
	[SerializeField]
	private UILabel _headerLabel;

	[SerializeField]
	private UILabel _titleLabel;

	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private UIWidget _currencyWidget;

	[SerializeField]
	private UISprite[] _currencyIcons;

	[SerializeField]
	private UILabel _prevCurrencyLabel;

	[SerializeField]
	private UILabel _nextCurrencyLabel;

	[SerializeField]
	private UIWidget _captionWidget;

	[SerializeField]
	private UILabel _captionLabel;

	[SerializeField]
	private SelectableButton _closeButton;

	[SerializeField]
	private SelectableButton _confirmButton;

	[SerializeField]
	private RectLayout _layout;

	private Durango.Logic.Shop.Commodity _commodity;

	private string _title;

	public override bool DragLock => true;

	protected override void Start()
	{
		base.Start();
		_closeButton.Text = T._("닫기");
		SelectableButton closeButton = _closeButton;
		closeButton.Clicked = (Action)Delegate.Combine(closeButton.Clicked, new Action(Hide));
		SelectableButton confirmButton = _confirmButton;
		confirmButton.Clicked = (Action)Delegate.Combine(confirmButton.Clicked, new Action(OnConfirm));
		_headerLabel.text = T._("아이템이 지급되었습니다.");
	}

	protected override void OnTryConfirmOnModal()
	{
		OnConfirm();
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _confirmButton;
	}

	protected override SelectableButton GetCancelButton(out bool showShortcut)
	{
		showShortcut = true;
		return _closeButton;
	}

	private void OnConfirm()
	{
		Hide();
		ShopGroup shopGroup = UIManager.FindScript<ShopGroup>();
		if (_commodity == null)
		{
			shopGroup.Open();
		}
		else if (_commodity.IsQuestPurchase(CommodityCondition.Type.Level))
		{
			Purchase questPurchase = _commodity.GetQuestPurchase(CommodityCondition.Type.Level);
			if (questPurchase != null)
			{
				SubCommoditiesPopup subCommoditiesPopup = UIManager.Popup.Tooltip<SubCommoditiesPopup>();
				subCommoditiesPopup.Set(questPurchase);
				subCommoditiesPopup.Show();
			}
		}
		else if ((_commodity.SalesTag & Tags.AcceptAutomatically) == 0)
		{
			shopGroup.OpenPurchases();
		}
	}

	public void Set(Durango.Logic.Shop.Commodity commodity, string title = null)
	{
		_commodity = commodity;
		_title = title;
	}

	protected override void FillData()
	{
		ItemIcon icon = _commodity.GetIcon(large: true);
		_iconTexture.SetIcon(icon);
		_iconTexture.gameObject.SetActive(value: true);
		_titleLabel.text = ((!string.IsNullOrEmpty(_title)) ? _title : _commodity.Title);
		FillCurrencyWidget();
		if (_commodity.IsQuestPurchase(CommodityCondition.Type.Level))
		{
			_confirmButton.Text = T._("확인");
			SetCaption(null);
			_closeButton.gameObject.SetActive(value: false);
		}
		else if ((_commodity.SalesTag & Tags.AcceptAutomatically) == 0)
		{
			_confirmButton.Text = T._("보관함 열기");
			SetCaption(T._("구매한 아이템은 보관함에서 확인 가능합니다."));
			_closeButton.gameObject.SetActive(value: true);
		}
		else
		{
			_confirmButton.Text = T._("확인");
			SetCaption(_commodity.PurchasedCaption);
			_closeButton.gameObject.SetActive(value: false);
		}
	}

	private void SetCaption(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			_captionWidget.gameObject.SetActive(value: false);
			return;
		}
		_captionWidget.gameObject.SetActive(value: true);
		_captionLabel.text = text;
	}

	private void FillCurrencyWidget()
	{
		int num = -1;
		int i = 0;
		for (int size = KUtility.GetSize(_commodity.Contents.Money); i < size; i++)
		{
			Currency currency = _commodity.Contents.Money[i].currency;
			if ((currency == Currency.Gem || currency == Currency.RPiece) && _commodity.Contents.Money[i].amount > 0)
			{
				num = i;
				break;
			}
		}
		if (num == -1)
		{
			_currencyWidget.gameObject.SetActive(value: false);
			return;
		}
		MoneyContent moneyContent = _commodity.Contents.Money[num];
		long balance = InventorySystem.Wallet.GetBalance(moneyContent.currency);
		_nextCurrencyLabel.text = Inventory.CurrencyFormat(balance);
		_prevCurrencyLabel.text = Inventory.CurrencyFormat(balance - moneyContent.amount);
		_currencyWidget.gameObject.SetActive(value: true);
		int j = 0;
		for (int size2 = KUtility.GetSize(_currencyIcons); j < size2; j++)
		{
			_currencyIcons[j].spriteName = Inventory.GetIcon(moneyContent.currency);
		}
	}

	protected override void UpdateLayout()
	{
		_layout.UpdateLayout();
		base.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
	}
}
