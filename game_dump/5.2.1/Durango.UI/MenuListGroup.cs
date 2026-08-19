using System;
using Durango.UI.Control;
using Durango.UI.Popup;
using Shared.Economy;
using Shared.Season2;
using UnityEngine;

namespace Durango.UI;

[Uri("Menu")]
public class MenuListGroup : MenuListGroupBase
{
	[SerializeField]
	private Transform _currencyContainer;

	[SerializeField]
	private CurrencyWidgetBase[] _currencyWidgets;

	[SerializeField]
	private GameObject _walletPopupButton;

	[SerializeField]
	private GameObject _walletFolded;

	[SerializeField]
	private GameObject _walletUnfolded;

	protected override void Start()
	{
		base.Start();
		UIEventListener uIEventListener = UIEventListener.Get(_walletPopupButton);
		uIEventListener.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener.onClick, (UIEventListener.VoidDelegate)delegate
		{
			_walletFolded.SetActive(value: false);
			_walletUnfolded.SetActive(value: true);
			WalletInfoPopup walletInfoPopup = UIManager.Popup.Tooltip<WalletInfoPopup>();
			walletInfoPopup.AddOnFinished(delegate
			{
				_walletFolded.SetActive(value: true);
				_walletUnfolded.SetActive(value: false);
			});
			walletInfoPopup.Show(_walletPopupButton.transform, Vector2.down * 50f);
		});
		UIEventListener uIEventListener2 = UIEventListener.Get(TouchBlockBox.gameObject);
		uIEventListener2.onClick = (UIEventListener.VoidDelegate)Delegate.Combine(uIEventListener2.onClick, (UIEventListener.VoidDelegate)delegate
		{
			Close();
		});
		UIEventListener uIEventListener3 = UIEventListener.Get(TouchBlockBox.gameObject);
		uIEventListener3.onDrag = (UIEventListener.VectorDelegate)Delegate.Combine(uIEventListener3.onDrag, (UIEventListener.VectorDelegate)delegate
		{
			Close();
			UIManager.SetCurrentUITouchEvent(enable: false);
		});
		_currencyContainer.gameObject.SetActive(value: false);
		InitCurrencyWidget();
	}

	protected override bool TryOpen()
	{
		if (!base.TryOpen())
		{
			return false;
		}
		_currencyContainer.gameObject.SetActive(value: true);
		return true;
	}

	protected override bool TryClose()
	{
		_currencyContainer.gameObject.SetActive(value: false);
		return base.TryClose();
	}

	private void InitCurrencyWidget()
	{
		if (GameManager.Region.IsWarpRush())
		{
			_currencyWidgets[0].gameObject.SetActive(value: false);
			_currencyWidgets[1].SetWarpRushResource(ResourceType.AlphaStone, total: false);
			_currencyWidgets[2].SetWarpRushResource(ResourceType.BravoStone, total: false);
		}
		else
		{
			_currencyWidgets[0].SetCurrencyType(Currency.Coin);
			_currencyWidgets[1].SetCurrencyType(Currency.Gem);
			_currencyWidgets[2].SetCurrencyType(Currency.TStone);
		}
	}
}
