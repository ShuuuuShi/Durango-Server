using System;
using Durango.Logic.Market;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using UnityEngine;

namespace Durango.UI;

public class MarketFavoritesButton : MonoBehaviour
{
	[SerializeField]
	private SelectableButton _favoritesButton;

	[SerializeField]
	private UILabel _description;

	[SerializeField]
	private TweenAlpha _descAnimation;

	private bool _blockClick;

	private void OnEnable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Combine(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
	}

	private void OnDisable()
	{
		UICamera.onPress = (UICamera.BoolDelegate)Delegate.Remove(UICamera.onPress, new UICamera.BoolDelegate(OnTouch));
	}

	private void OnTouch(GameObject go, bool press)
	{
		if (press && _descAnimation.enabled)
		{
			_descAnimation.enabled = false;
			_descAnimation.value = 0f;
		}
	}

	public void Set([CanBeNull] Commodity commodity, Action<Commodity> favoriteChanged)
	{
		if (commodity == null || string.IsNullOrEmpty(commodity.Id) || !GameSystem<MarketSystem>.Instance().IsFavoriteSystemOnline)
		{
			base.gameObject.SetActive(value: false);
			return;
		}
		base.gameObject.SetActive(value: true);
		_descAnimation.Sample(0f, isFinished: true);
		_favoritesButton.Selected = GameSystem<MarketSystem>.Instance().IsFavorite(commodity.Id);
		_blockClick = false;
		_favoritesButton.Clicked = delegate
		{
			if (!_blockClick)
			{
				_blockClick = true;
				_favoritesButton.Selected = !GameSystem<MarketSystem>.Instance().IsFavorite(commodity.Id);
				FavoriteAdded(commodity, favoriteChanged);
			}
		};
	}

	private void FavoriteAdded(Commodity commodity, Action<Commodity> favoriteChanged)
	{
		GameSystem<MarketSystem>.Instance().ToggleFavorite(commodity.Id, delegate(bool success, bool isAdded, string id)
		{
			if (!(commodity.Id != id))
			{
				_blockClick = false;
				_favoritesButton.Selected = isAdded;
				if (success)
				{
					_description.text = ((!isAdded) ? T._("찜 목록 등록이 취소되었습니다.") : T._("아이템이 찜 목록에 등록되었습니다. \n 내 장터 > 찜 목록에서 확인 가능합니다"));
					_descAnimation.tweenFactor = 0f;
					_descAnimation.PlayForward();
					if (favoriteChanged != null)
					{
						favoriteChanged(commodity);
					}
				}
			}
		});
	}
}
