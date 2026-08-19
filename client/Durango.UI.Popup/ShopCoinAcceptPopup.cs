using System;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.UI.Control;
using L10N;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI.Popup;

public class ShopCoinAcceptPopup : TooltipBase
{
	[SerializeField]
	private UIWidget _descriptionWidget;

	[SerializeField]
	private UILabel _descriptionText;

	[SerializeField]
	private UISprite[] _currencyIcons;

	[SerializeField]
	private UILabel _prevLabel;

	[SerializeField]
	private UILabel _nextLabel;

	[SerializeField]
	private SelectableButton _cancelButton;

	[SerializeField]
	private SelectableButton _acceptButton;

	[SerializeField]
	private RectLayout _layout;

	private Purchase _purchase;

	private Commodity _commodity;

	private Action _accepted;

	public override bool DragLock => true;

	protected override void Start()
	{
		base.Start();
		SelectableButton cancelButton = _cancelButton;
		cancelButton.Clicked = (Action)Delegate.Combine(cancelButton.Clicked, new Action(Hide));
		SelectableButton acceptButton = _acceptButton;
		acceptButton.Clicked = (Action)Delegate.Combine(acceptButton.Clicked, new Action(OnTryConfirmOnModal));
		int i = 0;
		for (int size = KUtility.GetSize(_currencyIcons); i < size; i++)
		{
			_currencyIcons[i].spriteName = Inventory.GetIcon(Currency.Coin);
		}
		_cancelButton.Text = T._("취소");
		_acceptButton.Text = T._("개봉");
	}

	public void Show(Purchase purchase, Action accepted)
	{
		_accepted = accepted;
		_purchase = purchase;
		_commodity = GameSystem<ShopSystem>.Instance().GetCommodity(purchase.CommodityId);
		if (_commodity != null && _commodity.IsProduct)
		{
			Show();
		}
	}

	protected override void OnTryConfirmOnModal()
	{
		Hide();
		if (_accepted != null)
		{
			_accepted();
		}
	}

	protected override SelectableButton GetConfirmButton(out bool showShortcut)
	{
		showShortcut = true;
		return _acceptButton;
	}

	protected override SelectableButton GetCancelButton(out bool showShortcut)
	{
		showShortcut = true;
		return _cancelButton;
	}

	protected override void FillData()
	{
		_descriptionText.text = string.Format("<em>{0}</em>\n[size=10] \n[/size][size=20][B8B09DC6]{1}[-][/size]", _purchase.GetName(), T._("개봉한 상품은 환불이 가능하지 않습니다.\n결제 문의는 [게임 내 고객 센터 > 1:1문의하기]로 접수해주세요."));
		long balance = InventorySystem.Wallet.GetBalance(Currency.Coin);
		_prevLabel.text = balance.ToString();
		long num = _commodity.CoinAmount;
		if (_commodity.IsFirstPurchaseBonus(_purchase.Id))
		{
			num += _commodity.Data.CoinFirstPurchaseBonus;
		}
		_nextLabel.text = (balance + num).ToString();
	}

	protected override void UpdateLayout()
	{
		_descriptionWidget.height = _descriptionText.height + 60;
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
		base.Widget.SetPosition(Vector3.zero, 0.5f, 0.5f);
	}
}
