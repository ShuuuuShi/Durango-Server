using Shared.Faction;
using UnityEngine;

namespace Durango.UI;

public class MapFactionIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _mainSprite;

	[SerializeField]
	private UISprite _subSprite;

	public FactionType FactionType { get; set; }

	public void SetIcon(string icon, Color color, int size, int depth)
	{
		_mainSprite.spriteName = icon;
		_mainSprite.color = color;
		UIUtility.ResizeToSquare(_mainSprite, size);
		_mainSprite.depth = depth;
		_subSprite.depth = depth + 1;
	}

	public void SetSubIcon(string icon, Color color, int size)
	{
		_subSprite.spriteName = icon;
		_subSprite.color = color;
		UIUtility.ResizeToSquare(_subSprite, size);
	}
}
