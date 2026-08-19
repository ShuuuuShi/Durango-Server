using System;
using UnityEngine;

[Serializable]
public struct SpriteData
{
	public string sprite;

	public Color color;

	public void Set(UISprite uiSprite)
	{
		if (!(uiSprite == null))
		{
			uiSprite.spriteName = sprite;
			if (color != Color.clear)
			{
				uiSprite.color = color;
			}
		}
	}
}
