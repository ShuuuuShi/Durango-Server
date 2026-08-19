using Durango.Logic.Item;
using L10N;
using Messages;
using Shared.Economy;
using UnityEngine;

namespace Durango.UI.Popup;

public class PaidCurrencyInfoWidget : UIWidget
{
	[SerializeField]
	private UILabel _currencyNameLabel;

	[SerializeField]
	private UISprite _currencyIconSprite;

	[SerializeField]
	private UILabel _totalAmountLabel;

	[SerializeField]
	private UILabel _paidLabel;

	[SerializeField]
	private UILabel _paidAmountLabel;

	[SerializeField]
	private UILabel _unpaidLabel;

	[SerializeField]
	private UILabel _unpaidAmountLabel;

	[SerializeField]
	private RectLayout _layout;

	protected override void OnStart()
	{
		base.OnStart();
		if (Application.isPlaying)
		{
			_paidLabel.text = T._("유료분");
			_unpaidLabel.text = T._("무료분");
			_layout.UpdateOnSizeChange();
		}
	}

	public void Set(Currency type)
	{
		_currencyNameLabel.text = type.Normalize().GetName();
		_currencyIconSprite.spriteName = Durango.Logic.Item.Inventory.GetIcon(type);
		Wallet wallet = InventorySystem.Wallet;
		_totalAmountLabel.text = Durango.Logic.Item.Inventory.CurrencyFormat(wallet.GetBalance(type));
		_paidAmountLabel.text = Durango.Logic.Item.Inventory.CurrencyFormat(wallet.GetPaidBalance(type));
		_unpaidAmountLabel.text = Durango.Logic.Item.Inventory.CurrencyFormat(wallet.GetUnpaidBalance(type));
	}
}
