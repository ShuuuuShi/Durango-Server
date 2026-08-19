using System;
using Durango.Logic.Item;
using Durango.Logic.Market;
using Durango.UI.Control;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class CommodityListBottomBar : MonoBehaviour
{
	[SerializeField]
	private UIWidget _widget;

	[SerializeField]
	private SelectableButton _buyButton;

	[SerializeField]
	private AnimationWidget _animationWidget;

	[SerializeField]
	private SelectableButton _searchButton;

	[SerializeField]
	private MarketFavoritesButton _favoritesButton;

	private bool _isShow;

	private bool _isInit;

	public event Action BuyButtonClicked;

	public event Action PrototypeSearchClicked;

	private void Init()
	{
		if (_isInit)
		{
			return;
		}
		_isInit = true;
		SelectableButton buyButton = _buyButton;
		buyButton.Clicked = (Action)Delegate.Combine(buyButton.Clicked, (Action)delegate
		{
			if (this.BuyButtonClicked != null)
			{
				this.BuyButtonClicked();
			}
		});
		SelectableButton searchButton = _searchButton;
		searchButton.Clicked = (Action)Delegate.Combine(searchButton.Clicked, (Action)delegate
		{
			if (this.PrototypeSearchClicked != null)
			{
				this.PrototypeSearchClicked();
			}
		});
		Point2 preferredSize = _searchButton.GetPreferredSize();
		_searchButton.SetDimensions(preferredSize.x, _searchButton.Widget.height);
		GetComponent<RectLayoutComponent>().UpdateOnSizeChange();
	}

	private void Start()
	{
		if (!_isShow)
		{
			_widget.alpha = 0f;
			base.gameObject.SetActive(value: false);
		}
	}

	public void Show(Commodity commodity, Action<Commodity> favoriteChanged)
	{
		Init();
		_isShow = true;
		if (commodity == null)
		{
			Hide();
			return;
		}
		if (_buyButton != null)
		{
			_buyButton.Text = string.Format(T._("{0} 구매"), Inventory.CurrencyFormat(commodity.Price, commodity.CurrencyType));
			_buyButton.Disabled = commodity.Price > InventorySystem.Wallet.GetBalance(commodity.CurrencyType);
		}
		base.gameObject.SetActive(value: true);
		_animationWidget.SetAlpha(1f);
		_favoritesButton.Set(commodity, favoriteChanged);
	}

	public void Hide()
	{
		_isShow = false;
		base.gameObject.SetActive(value: true);
		_animationWidget.SetAlpha(0f);
	}
}
