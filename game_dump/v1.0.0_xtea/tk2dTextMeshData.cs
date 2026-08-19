using System;
using UnityEngine;

[Serializable]
public class tk2dTextMeshData
{
	public int version;

	public tk2dFontData font;

	public string text = string.Empty;

	public Color color = Color.white;

	public Color color2 = Color.white;

	public bool useGradient;

	public int textureGradient;

	public TextAnchor anchor = (TextAnchor)6;

	public int renderLayer;

	public Vector3 scale = Vector3.one;

	public bool kerning;

	public int maxChars = 16;

	public bool inlineStyling;

	public bool formatting;

	public int wordWrapWidth;

	public float spacing;

	public float lineSpacing;
}
