using System;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public class PurchasedPage : UIWidget
{
	[SerializeField]
	private PurchasedListWidget _purchasedList;

	[SerializeField]
	private UILabel _mileageLabel;

	[SerializeField]
	private UILabel _mileageAmountLabel;

	[SerializeField]
	private SelectableButton _closeButton;

	[SerializeField]
	private SelectableButton _moveToPurchasedButton;

	[SerializeField]
	private SelectableButton _retryButton;

	[SerializeField]
	private UIWidget _actionBarBackground;

	[SerializeField]
	private UIWidget _actionBarSeperator;

	private ShopGroup _parent;

	private Durango.Logic.Shop.Commodity _commodity;

	private bool _isInit;

	public bool IsShow { get; private set; }

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		_parent = UIUtility.FindComponentInParent<ShopGroup>(base.gameObject);
		_moveToPurchasedButton.Text = T._("보관함 열기");
		_closeButton.Text = T._("나가기");
		SelectableButton moveToPurchasedButton = _moveToPurchasedButton;
		moveToPurchasedButton.Clicked = (Action)Delegate.Combine(moveToPurchasedButton.Clicked, (Action)delegate
		{
			_parent.HidePurchasedPage();
			_parent.OpenPurchases();
		});
		SelectableButton closeButton = _closeButton;
		closeButton.Clicked = (Action)Delegate.Combine(closeButton.Clicked, (Action)delegate
		{
			_parent.HidePurchasedPage();
		});
		SelectableButton retryButton = _retryButton;
		retryButton.Clicked = (Action)Delegate.Combine(retryButton.Clicked, (Action)delegate
		{
			_parent.BuyCommodity(_commodity);
		});
		if (_actionBarBackground != null)
		{
			Transform target = _actionBarBackground.transform.parent;
			if (UIManager.SafeArea.width < 1f)
			{
				_actionBarBackground.leftAnchor.SetScreen(0f, 0f);
				_actionBarBackground.rightAnchor.SetScreen(1f, 0f);
			}
			else
			{
				_actionBarBackground.leftAnchor.Set(target, 0f, 0f);
				_actionBarBackground.rightAnchor.Set(target, 1f, 0f);
			}
			if (UIManager.SafeArea.yMax < 1f)
			{
				_actionBarBackground.bottomAnchor.SetScreen(0f, 0f);
			}
			else
			{
				_actionBarBackground.bottomAnchor.Set(target, 0f, 0f);
			}
			_actionBarBackground.ResetAndUpdateAnchors();
		}
		if (_actionBarSeperator != null)
		{
			if (UIManager.SafeArea.width < 1f)
			{
				_actionBarSeperator.leftAnchor.SetScreen(0f, 0f);
				_actionBarSeperator.rightAnchor.SetScreen(1f, 0f);
			}
			else
			{
				Transform target2 = _actionBarSeperator.transform.parent;
				_actionBarSeperator.leftAnchor.Set(target2, 0f, 0f);
				_actionBarSeperator.rightAnchor.Set(target2, 1f, 0f);
			}
			_actionBarBackground.ResetAndUpdateAnchors();
		}
		_mileageLabel.text = $"[icon={Durango.Logic.Item.Inventory.GetIcon(Currency.CashshopMileage)}] {Currency.CashshopMileage.GetName()}";
		Vector3[] array = _mileageLabel.transform.parent.GetComponent<UIWidget>().localCorners;
		Vector3 pos = Vector3.Lerp(array[0], array[1], 0.5f);
		pos.x += 20f;
		_mileageLabel.SetPosition(pos, 0f, 0.5f);
		pos.x += (float)_mileageLabel.width + 10f;
		_mileageAmountLabel.SetPosition(pos, 0f, 0.5f);
	}

	public void Show(Durango.Logic.Shop.Commodity commodity, Purchased purchased, bool withVoucher)
	{
		Init();
		_commodity = commodity;
		IsShow = true;
		_purchasedList.Set(commodity, purchased);
		base.gameObject.SetActive(value: true);
		_retryButton.Text = string.Format("{0}  [preset=round_box?{1}]", T._("재구매"), GetPaymentMethod(commodity));
		long balance = InventorySystem.Wallet.GetBalance(Currency.CashshopMileage);
		long num = commodity.Data.BonusMileage;
		if (num > 0 && !withVoucher)
		{
			_mileageAmountLabel.text = string.Format(T.Culture, "{0:N0} <em>(+{1:N0})</em>", balance - num, num);
		}
		else
		{
			_mileageAmountLabel.text = string.Format(T.Culture, "{0:N0}", balance);
		}
	}

	private string GetPaymentMethod(Durango.Logic.Shop.Commodity commodity)
	{
		if (commodity.VoucherPurchasable() && InventorySystem.Wallet.PurchasableVoucherCount(commodity) > 0)
		{
			return $"{SingletonDict<string, Voucher>.Get(commodity.Data.VoucherId).GetIconText()} {commodity.Data.VoucherAmount}";
		}
		return commodity.GetCurrencyText(hasDiscountRatio: false);
	}

	public void Hide()
	{
		IsShow = false;
		base.gameObject.SetActive(value: false);
	}
}
