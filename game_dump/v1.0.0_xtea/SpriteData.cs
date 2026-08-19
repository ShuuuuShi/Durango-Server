using System;
using UnityEngine;

[Serializable]
public struct SpriteData
{
	public UIAtlas atlas;

	public string sprite;

	public Color color;

	public void Set(UISprite uiSprite)
	{
		//IL_003c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0041: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)uiSprite == (Object)null))
		{
			if ((Object)(object)uiSprite.atlas != (Object)(object)atlas)
			{
				uiSprite.atlas = atlas;
			}
			uiSprite.spriteName = sprite;
			if (color != Color.clear)
			{
				uiSprite.color = color;
			}
		}
	}
}
