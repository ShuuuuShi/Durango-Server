using System;
using UnityEngine;

[Serializable]
public class tk2dSpriteCollectionFont
{
	public bool active;

	public TextAsset bmFont;

	public Texture2D texture;

	public bool dupeCaps;

	public bool flipTextureY;

	public int charPadX;

	public tk2dFontData data;

	public tk2dFont editorData;

	public int materialId;

	public bool useGradient;

	public Texture2D gradientTexture;

	public int gradientCount = 1;

	public string Name
	{
		get
		{
			if ((Object)(object)bmFont == (Object)null || (Object)(object)texture == (Object)null)
			{
				return "Empty";
			}
			if ((Object)(object)data == (Object)null)
			{
				return ((Object)bmFont).name + " (Inactive)";
			}
			return ((Object)bmFont).name;
		}
	}

	public bool InUse => active && (Object)(object)bmFont != (Object)null && (Object)(object)texture != (Object)null && (Object)(object)data != (Object)null && (Object)(object)editorData != (Object)null;

	public void CopyFrom(tk2dSpriteCollectionFont src)
	{
		active = src.active;
		bmFont = src.bmFont;
		texture = src.texture;
		dupeCaps = src.dupeCaps;
		flipTextureY = src.flipTextureY;
		charPadX = src.charPadX;
		data = src.data;
		editorData = src.editorData;
		materialId = src.materialId;
		gradientCount = src.gradientCount;
		gradientTexture = src.gradientTexture;
		useGradient = src.useGradient;
	}
}
