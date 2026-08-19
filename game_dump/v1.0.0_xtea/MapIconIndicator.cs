using UnityEngine;

public class MapIconIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _sprite;

	public void SetIcon(string icon, Color color, int size, int depth)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_sprite.spriteName = icon;
		_sprite.color = color;
		UIUtility.ResizeToSquare(_sprite, size);
		_sprite.depth = depth;
	}
}
