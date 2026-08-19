using System;
using Durango.Logic.Social;
using Durango.UI.Control;
using Durango.Utils.Extensions;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.UI;

public class FavoritesMotionWidget : MotionWidget
{
	[SerializeField]
	private SelectableWidget _favoritesSelection;

	[SerializeField]
	private UISprite _starMarkSprite;

	public void Set([CanBeNull] Durango.Logic.Social.Motion data, [CanBeNull] Action favoritesClicked, [CanBeNull] Action motionClicked)
	{
		Set(data, motionClicked);
		if (data != null && data.IsEquipmentsMotion())
		{
			_favoritesSelection.gameObject.SetActive(value: false);
			return;
		}
		_favoritesSelection.gameObject.SetActive(value: true);
		_favoritesSelection.Clicked = favoritesClicked;
		_starMarkSprite.spriteName = ((data == null || !data.Favorite) ? "autoguide_icon_star_active2" : "autoguide_icon_star_active");
		_starMarkSprite.color = _starMarkSprite.color.WithA((data == null || !data.Available) ? 0.3f : 1f);
	}
}
