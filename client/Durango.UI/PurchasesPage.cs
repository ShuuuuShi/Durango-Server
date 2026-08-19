using System;
using System.Collections.Generic;
using Durango.Logic.Shop;
using Durango.System;
using Durango.UI.Control;
using Durango.UI.Popup;
using L10N;
using UnityEngine;
using Yaml;

namespace Durango.UI;

public class PurchasesPage : MonoBehaviour
{
	[SerializeField]
	private GameObject _mainList;

	[SerializeField]
	private KInfiniteScrollView _scrollView;

	[SerializeField]
	private UILabel _warningLabel;

	[SerializeField]
	private SelectableButton _buttonBase;

	[SerializeField]
	private GameObject _noData;

	private KInfiniteScrollView.View<Purchase, PurchaseWidget> _view;

	private readonly List<Purchase> _list = new List<Purchase>();

	private ListObjectPool<SelectableButton> _buttons;

	private RectLayoutComponent _layout;

	private bool _isInit;

	private void Init()
	{
		if (!_isInit)
		{
			_isInit = true;
			_view = _scrollView.Initialize(delegate(PurchaseWidget comp, Purchase data)
			{
				comp.Set(data);
			}, delegate(PurchaseWidget comp)
			{
				comp.Clicked = (Action<Purchase>)Delegate.Combine(comp.Clicked, new Action<Purchase>(OnClickPurchase));
			});
			_view.SetList(_list);
			_warningLabel.text = T._("상품은 시간이 지나면 사라집니다.");
			_buttons = new ListObjectPool<SelectableButton>();
			_buttons.BaseObject = _buttonBase;
			UpdateButtons();
			_layout = GetComponent<RectLayoutComponent>();
		}
	}

	private void UpdateButtons()
	{
		_buttons.BeginLoad();
		if (Platform.Instance.Country == NPCountry.Japan)
		{
			SelectableButton next = _buttons.GetNext();
			next.Text = T._("소지재화 확인");
			next.Clicked = (Action)Delegate.Combine(next.Clicked, (Action)delegate
			{
				PaidCurrencyInfoPopup paidCurrencyInfoPopup = UIManager.Popup.Tooltip<PaidCurrencyInfoPopup>();
				paidCurrencyInfoPopup.DefaultSetting().Show();
			});
		}
		SelectableButton next2 = _buttons.GetNext();
		next2.Text = T._("이용권 보기");
		next2.Clicked = (Action)Delegate.Combine(next2.Clicked, (Action)delegate
		{
			ShopVouchersPopup shopVouchersPopup = UIManager.Popup.Tooltip<ShopVouchersPopup>();
			shopVouchersPopup.Show();
		});
		_buttons.EndLoad();
		_buttons.Reposition(Vector3.left, 10);
	}

	public void Show(bool reset)
	{
		Init();
		_list.Clear();
		List<Purchase> purchases = GameSystem<ShopSystem>.Instance().Purchases;
		if (purchases.Count > 0)
		{
			foreach (Purchase item in purchases)
			{
				if (item.HasSubCommodities && item.SubCommodityConditionType == CommodityCondition.Type.Level)
				{
					_list.Add(item);
				}
			}
			foreach (Purchase item2 in purchases)
			{
				if (!item2.HasSubCommodities)
				{
					_list.Add(item2);
				}
			}
			if (reset)
			{
				_scrollView.ResetPosition();
			}
			else
			{
				_scrollView.Reposition();
			}
			_mainList.gameObject.SetActive(value: true);
			_noData.gameObject.SetActive(value: false);
		}
		else
		{
			_mainList.gameObject.SetActive(value: false);
			_noData.gameObject.SetActive(value: true);
		}
		base.gameObject.SetActive(value: true);
		_layout.UpdateLayout();
		UIUtility.UpdateAnchors(base.transform);
	}

	public void Hide()
	{
		base.gameObject.SetActive(value: false);
	}

	private void OnClickPurchase(Purchase purchase)
	{
		if (purchase.HasSubCommodities)
		{
			ShopGroup.ShowSubCommodityStatus(purchase);
			return;
		}
		Durango.Logic.Shop.Commodity commodity = GameSystem<ShopSystem>.Instance().GetCommodity(purchase.CommodityId);
		if (commodity != null && commodity.CoinAmount > 0)
		{
			ShopCoinAcceptPopup shopCoinAcceptPopup = UIManager.Popup.Tooltip<ShopCoinAcceptPopup>();
			shopCoinAcceptPopup.Show(purchase, delegate
			{
				GameSystem<ShopSystem>.Instance().AcceptPurchase(purchase.Id, delegate(bool ok)
				{
					if (ok)
					{
						SoundManager.PlayEvent("ui_menu_store_item_recieve");
					}
				});
			});
			return;
		}
		string acceptPurchaseDesc = purchase.GetAcceptPurchaseDescription();
		GameSystem<ShopSystem>.Instance().AcceptPurchase(purchase.Id, delegate(bool ok)
		{
			if (ok)
			{
				SoundManager.PlayEvent("ui_menu_store_item_recieve");
				UIManager.SystemMsg("AcceptPurchase", acceptPurchaseDesc);
			}
		});
	}
}
