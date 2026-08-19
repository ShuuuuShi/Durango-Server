using Durango.UI.Control;
using UnityEngine;

namespace Durango.UI;

public class MapIconIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _sprite;

	public void SetIcon(string icon, Color color, int size, int depth)
	{
		_sprite.spriteName = icon;
		_sprite.color = color;
		UIUtility.ResizeToSquare(_sprite, size);
		_sprite.depth = depth;
	}

	public void StartTweener(float delay)
	{
		TweenerPlayer component = GetComponent<TweenerPlayer>();
		if (component != null)
		{
			component.Play(delay);
		}
	}
}
