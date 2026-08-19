using System;
using Durango.Logic.Item;
using Durango.Logic.Shop;
using Durango.UI.Control;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI;

public class PurchaseWidget : MonoBehaviour
{
	public Action<Durango.Logic.Shop.Purchase> Clicked;

	[SerializeField]
	private GameObject _newMaker;

	[SerializeField]
	private ItemIconTex _iconTexture;

	[SerializeField]
	private GameObject _newIconObject;

	[SerializeField]
	private UILabel _mainText;

	[SerializeField]
	private UILabel _subText;

	[SerializeField]
	private SelectableButton _button;

	private Durango.Logic.Shop.Purchase _purchase;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			SelectableButton button = _button;
			button.Clicked = (Action)Delegate.Combine(button.Clicked, new Action(OnClickButton));
		}
	}

	public void Set(Durango.Logic.Shop.Purchase purchase)
	{
		Init();
		_purchase = purchase;
		if (_purchase.GetPayBackMileage(out var paybackMileage))
		{
			_newIconObject.gameObject.SetActive(value: false);
			if (paybackMileage > 0)
			{
				_mainText.text = purchase.GetName() + " <weak>+" + Durango.Logic.Item.Inventory.CurrencyFormat(paybackMileage, Currency.CashshopMileage) + "</weak>";
			}
			else
			{
				_mainText.text = purchase.GetName();
			}
		}
		else
		{
			_newIconObject.gameObject.SetActive(value: true);
			_mainText.text = purchase.GetName();
		}
		_iconTexture.SetIcon(purchase.GetIcon());
		string text2 = null;
		PresetButton.Style style = PresetButton.Style.Solid;
		if (purchase.HasSubCommodities)
		{
			_subText.gameObject.SetActive(value: false);
			AcceptableSubPurchase? acceptableSubPurchase = GameSystem<ShopSystem>.Instance().GetAcceptableSubPurchase(purchase.Id);
			bool flag = false;
			if (acceptableSubPurchase.HasValue)
			{
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
			if (flag)
			{
				text2 = T._("받기");
			}
			else
			{
				text2 = T._("보기");
				style = PresetButton.Style.Border;
			}
			_newMaker.gameObject.SetActive(value: false);
		}
		else
		{
			if (purchase.Item != null)
			{
				text2 = T._("받기");
			}
			else if (purchase.Emotion != null)
			{
				text2 = T._("받기");
			}
			else
			{
				Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(purchase.CommodityId);
				text2 = ((commodity == null || !commodity.IsProduct) ? T._("받기") : T._("개봉"));
			}
			if (purchase.ExpiresAt > 0.0)
			{
				_subText.gameObject.SetActive(value: true);
				_subText.SetText(new SyncString(delegate(out string text, out float period)
				{
					SyncString.UpdateRemainTimeMsg(purchase.ExpiresAt, T._("{0} 내 수령 필요"), out text, out period, string.Empty);
				}));
			}
			else
			{
				_subText.gameObject.SetActive(value: false);
			}
			_newMaker.gameObject.SetActive(value: true);
		}
		_button.Text = text2;
		_button.SetStyle(style);
	}

	private void OnClickButton()
	{
		if (Clicked != null)
		{
			Clicked(_purchase);
		}
	}
}
