using System;
using ItemSystem;
using MarketData;
using UnityEngine;

public class CommodityListBottomBar : MonoBehaviour
{
	[SerializeField]
	private DefaultSelectableButton _buyButton;

	[SerializeField]
	private Selectable _similarItemButton;

	[SerializeField]
	private Selectable _personalMarketButton;

	private string _buyButtonTextFormat;

	private bool _isShow;

	private bool _isInit;

	public event Action BuyButtonClicked;

	public event Action SimilarButtonClicked;

	public event Action PersonalMarketButtonClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		if ((Object)(object)_buyButton != (Object)null)
		{
			_buyButtonTextFormat = _buyButton.Text;
			DefaultSelectableButton buyButton = _buyButton;
			buyButton.Clicked = (Action)Delegate.Combine(buyButton.Clicked, (Action)delegate
			{
				if (!_buyButton.Disable && this.BuyButtonClicked != null)
				{
					this.BuyButtonClicked();
				}
			});
		}
		if ((Object)(object)_similarItemButton != (Object)null)
		{
			Selectable similarItemButton = _similarItemButton;
			similarItemButton.Clicked = (Action)Delegate.Combine(similarItemButton.Clicked, (Action)delegate
			{
				if (this.SimilarButtonClicked != null)
				{
					this.SimilarButtonClicked();
				}
			});
		}
		if (!((Object)(object)_personalMarketButton != (Object)null))
		{
			return;
		}
		Selectable personalMarketButton = _personalMarketButton;
		personalMarketButton.Clicked = (Action)Delegate.Combine(personalMarketButton.Clicked, (Action)delegate
		{
			if (this.PersonalMarketButtonClicked != null)
			{
				this.PersonalMarketButtonClicked();
			}
		});
	}

	private void Start()
	{
		if (!_isShow)
		{
			((Component)this).gameObject.SetActive(false);
		}
	}

	public void Show(Commodity commodity)
	{
		Init();
		_isShow = true;
		if (commodity == null)
		{
			Hide();
			return;
		}
		if ((Object)(object)_buyButton != (Object)null)
		{
			_buyButton.Text = string.Format(_buyButtonTextFormat, Inventory.CurrencyFormat(commodity.Price, commodity.CurrencyType));
			_buyButton.Disable = commodity.Price > GameSystem<InventorySystem>.Instance().PlayerInventory.GetBalance(commodity.CurrencyType);
		}
		((Component)this).gameObject.SetActive(true);
	}

	public void Hide()
	{
		_isShow = false;
		((Component)this).gameObject.SetActive(false);
	}
}
