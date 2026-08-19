using System;
using Durango.Logic.Social;
using Durango.Utils.Extensions;
using UnityEngine;

namespace Durango.UI;

public class FavoritesEmoticonWidget : EmoticonWidget
{
	[SerializeField]
	private UISprite _starMarkSprite;

	public override void Set(Emoticon emoticon, Action clicked)
	{
		base.Set(emoticon, clicked);
		_starMarkSprite.spriteName = ((!emoticon.Favorite) ? "craft_icon_star_disable_big" : "autoguide_icon_star_active");
		_starMarkSprite.color = _starMarkSprite.color.WithA((!emoticon.Available) ? 0.3f : 1f);
	}
}
