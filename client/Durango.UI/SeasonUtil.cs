using Durango.Logic;
using Messages;

namespace Durango.UI;

public static class SeasonUtil
{
	public static void SetSmallIcon(UISprite sprite, string season)
	{
		SetIcon(sprite, season, small: true);
	}

	public static void SetLargeIcon(UISprite sprite, string season)
	{
		SetIcon(sprite, season, small: false);
	}

	private static void SetIcon(UISprite sprite, string season, bool small)
	{
		if (!(sprite == null))
		{
			Season? season2 = GameSystem<SeasonSystem>.Instance().GetSeason(season);
			if (!season2.HasValue)
			{
				sprite.gameObject.SetActive(value: false);
				return;
			}
			sprite.gameObject.SetActive(value: true);
			sprite.spriteName = ((!small) ? season2.Value.IconLarge : season2.Value.IconSmall);
			UIUtility.ResizeToSquare(sprite);
		}
	}
}
