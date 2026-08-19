using Shared.Faction;
using UnityEngine;

public class MapFactionIndicator : MapIndicator
{
	[SerializeField]
	private UISprite _mainSprite;

	[SerializeField]
	private UISprite _subSprite;

	public FactionType FactionType { get; set; }

	public void SetIcon(string icon, Color color, int size, int depth)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_mainSprite.spriteName = icon;
		_mainSprite.color = color;
		UIUtility.ResizeToSquare(_mainSprite, size);
		_mainSprite.depth = depth;
		_subSprite.depth = depth + 1;
	}

	public void SetSubIcon(string icon, Color color, int size)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		_subSprite.spriteName = icon;
		_subSprite.color = color;
		UIUtility.ResizeToSquare(_subSprite, size);
	}
}
