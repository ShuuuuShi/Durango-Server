using ItemSystem;
using Shared.Economy;
using UnityEngine;

public class CurrencyLabel : MonoBehaviour
{
	[SerializeField]
	private Currency _currencyType;

	private UILabel _label;

	private UISpriteLabel _spriteLabel;

	private void Awake()
	{
		_label = ((Component)this).GetComponent<UILabel>();
		_spriteLabel = ((Component)this).GetComponent<UISpriteLabel>();
	}

	private void OnEnable()
	{
		GameSystem<InventorySystem>.Instance().PlayerBalanceUpdated += OnUpdateBalance;
		OnUpdateBalance();
	}

	private void OnDisable()
	{
		GameSystem<InventorySystem>.Instance().PlayerBalanceUpdated -= OnUpdateBalance;
	}

	private void OnUpdateBalance()
	{
		Inventory playerInventory = GameSystem<InventorySystem>.Instance().PlayerInventory;
		long balance = playerInventory.GetBalance(_currencyType);
		if ((Object)(object)_spriteLabel != (Object)null)
		{
			_spriteLabel.text = Inventory.CurrencyFormat(balance, _currencyType);
		}
		else if ((Object)(object)_label != (Object)null)
		{
			_label.text = Inventory.CurrencyFormat(balance, Currency.Invalid);
		}
	}
}
