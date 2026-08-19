using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class NGUIText
{
	public enum Alignment
	{
		Automatic,
		Left,
		Center,
		Right,
		Justified
	}

	public enum SymbolStyle
	{
		None,
		Normal,
		Colored
	}

	public class GlyphInfo
	{
		public Vector2 v0;

		public Vector2 v1;

		public Vector2 u0;

		public Vector2 u1;

		public Vector2 u2;

		public Vector2 u3;

		public float advance;

		public int channel;
	}

	public static UIFont bitmapFont;

	public static Font dynamicFont;

	public static GlyphInfo glyph = new GlyphInfo();

	public static int fontSize = 16;

	public static float fontScale = 1f;

	public static float pixelDensity = 1f;

	private static FontStyle _fontStyle = (FontStyle)0;

	public static Alignment alignment = Alignment.Left;

	public static Color tint = Color.white;

	public static int rectWidth = 1000000;

	public static int rectHeight = 1000000;

	public static int regionWidth = 1000000;

	public static int regionHeight = 1000000;

	public static int maxLines = 0;

	public static bool gradient = false;

	public static Color gradientBottom = Color.white;

	public static Color gradientTop = Color.white;

	public static bool encoding = false;

	public static float spacingX = 0f;

	public static float spacingY = 0f;

	public static bool premultiply = false;

	public static SymbolStyle symbolStyle;

	public static int finalSize = 0;

	public static float finalSpacingX = 0f;

	public static float finalLineHeight = 0f;

	public static float baseline = 0f;

	public static bool useSymbols = false;

	public static Stack<int> fontSizeStack = new Stack<int>();

	private static Color mInvisible = new Color(0f, 0f, 0f, 0f);

	private static BetterList<Color> mColors = new BetterList<Color>();

	private static float mAlpha = 1f;

	private static CharacterInfo mTempChar;

	private static BetterList<float> mSizes = new BetterList<float>();

	private static Color s_c0;

	private static Color s_c1;

	private static float[] mBoldOffset = new float[8] { -0.25f, 0f, 0.25f, 0f, 0f, -0.25f, 0f, 0.25f };

	public static FontStyle fontStyle
	{
		get
		{
			return (FontStyle)0;
		}
		set
		{
			//IL_0000: Unknown result type (might be due to invalid IL or missing references)
			//IL_0001: Unknown result type (might be due to invalid IL or missing references)
			_fontStyle = value;
		}
	}

	public static void Update()
	{
		Update(request: true);
	}

	public static void Update(bool request)
	{
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b3: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e2: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f9: Unknown result type (might be due to invalid IL or missing references)
		finalSize = Mathf.RoundToInt((float)fontSize / pixelDensity);
		finalSpacingX = spacingX * fontScale;
		finalLineHeight = ((float)fontSize + spacingY) * fontScale;
		useSymbols = (Object)(object)bitmapFont != (Object)null && bitmapFont.hasSymbols && encoding && symbolStyle != SymbolStyle.None;
		Font val = dynamicFont;
		if (!((Object)(object)val != (Object)null) || !request)
		{
			return;
		}
		val.RequestCharactersInTexture(")_-", finalSize, fontStyle);
		if (!val.GetCharacterInfo(')', ref mTempChar, finalSize, fontStyle) || (float)((CharacterInfo)(ref mTempChar)).maxY == 0f)
		{
			val.RequestCharactersInTexture("A", finalSize, fontStyle);
			if (!val.GetCharacterInfo('A', ref mTempChar, finalSize, fontStyle))
			{
				baseline = 0f;
				return;
			}
		}
		float num = ((CharacterInfo)(ref mTempChar)).maxY;
		float num2 = ((CharacterInfo)(ref mTempChar)).minY;
		baseline = Mathf.Round(num + ((float)finalSize - num + num2) * 0.5f);
	}

	public static void Prepare(string text)
	{
		//IL_0110: Unknown result type (might be due to invalid IL or missing references)
		//IL_00f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_006c: Unknown result type (might be due to invalid IL or missing references)
		if (!((Object)(object)dynamicFont != (Object)null))
		{
			return;
		}
		if (encoding)
		{
			fontSizeStack.Clear();
			int length = text.Length;
			int size = finalSize;
			int num = size;
			int num2 = 0;
			int num3 = 0;
			for (int i = 0; i < length; i++)
			{
				num3 = i;
				int space;
				if (ParseSize(text, ref i, ref size))
				{
					if (num2 < num3)
					{
						dynamicFont.RequestCharactersInTexture(text.Substring(num2, num3 - num2), num, fontStyle);
					}
					num = size;
					num2 = i;
					i--;
				}
				else if (ParseSpace(text, size, ref i, out space))
				{
					dynamicFont.RequestCharactersInTexture(" ", num, fontStyle);
					i--;
				}
				else if (ParseSymbol(text, ref i))
				{
					i--;
				}
			}
			num3++;
			if (num2 < num3)
			{
				dynamicFont.RequestCharactersInTexture(text.Substring(num2, num3 - num2), size, fontStyle);
			}
		}
		else
		{
			dynamicFont.RequestCharactersInTexture(text, finalSize, fontStyle);
		}
	}

	public static BMSymbol GetSymbol(string text, int index, int textLength)
	{
		return (!((Object)(object)bitmapFont != (Object)null)) ? null : bitmapFont.MatchSymbol(text, index, textLength);
	}

	public static float GetGlyphWidth(int ch, int prev)
	{
		return GetGlyphWidth(ch, prev, finalSize);
	}

	public static float GetGlyphWidth(int ch, int prev, int fontSize)
	{
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)bitmapFont != (Object)null)
		{
			bool flag = false;
			if (ch == 8201)
			{
				flag = true;
				ch = 32;
			}
			BMGlyph bMGlyph = bitmapFont.bmFont.GetGlyph(ch);
			if (bMGlyph != null)
			{
				int num = bMGlyph.advance;
				if (flag)
				{
					num >>= 1;
				}
				return fontScale * (float)((prev == 0) ? bMGlyph.advance : (num + bMGlyph.GetKerning(prev)));
			}
		}
		else if ((Object)(object)dynamicFont != (Object)null && dynamicFont.GetCharacterInfo((char)ch, ref mTempChar, fontSize, fontStyle))
		{
			return (float)((CharacterInfo)(ref mTempChar)).advance * fontScale * pixelDensity;
		}
		return 0f;
	}

	public static GlyphInfo GetGlyph(int ch, int prev)
	{
		return GetGlyph(ch, prev, finalSize);
	}

	public static GlyphInfo GetGlyph(int ch, int prev, int size)
	{
		//IL_0268: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02fa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0309: Unknown result type (might be due to invalid IL or missing references)
		//IL_030e: Unknown result type (might be due to invalid IL or missing references)
		//IL_031d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Unknown result type (might be due to invalid IL or missing references)
		//IL_0331: Unknown result type (might be due to invalid IL or missing references)
		//IL_0336: Unknown result type (might be due to invalid IL or missing references)
		//IL_0406: Unknown result type (might be due to invalid IL or missing references)
		//IL_040d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0412: Unknown result type (might be due to invalid IL or missing references)
		//IL_041d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0424: Unknown result type (might be due to invalid IL or missing references)
		//IL_0429: Unknown result type (might be due to invalid IL or missing references)
		//IL_01fc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0206: Unknown result type (might be due to invalid IL or missing references)
		//IL_020b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0216: Unknown result type (might be due to invalid IL or missing references)
		//IL_0220: Unknown result type (might be due to invalid IL or missing references)
		//IL_0225: Unknown result type (might be due to invalid IL or missing references)
		if ((Object)(object)bitmapFont != (Object)null)
		{
			bool flag = false;
			if (ch == 8201)
			{
				flag = true;
				ch = 32;
			}
			BMGlyph bMGlyph = bitmapFont.bmFont.GetGlyph(ch);
			if (bMGlyph != null)
			{
				int num = ((prev != 0) ? bMGlyph.GetKerning(prev) : 0);
				glyph.v0.x = ((prev == 0) ? bMGlyph.offsetX : (bMGlyph.offsetX + num));
				glyph.v1.y = -bMGlyph.offsetY;
				glyph.v1.x = glyph.v0.x + (float)bMGlyph.width;
				glyph.v0.y = glyph.v1.y - (float)bMGlyph.height;
				glyph.u0.x = bMGlyph.x;
				glyph.u0.y = bMGlyph.y + bMGlyph.height;
				glyph.u2.x = bMGlyph.x + bMGlyph.width;
				glyph.u2.y = bMGlyph.y;
				glyph.u1.x = glyph.u0.x;
				glyph.u1.y = glyph.u2.y;
				glyph.u3.x = glyph.u2.x;
				glyph.u3.y = glyph.u0.y;
				int num2 = bMGlyph.advance;
				if (flag)
				{
					num2 >>= 1;
				}
				glyph.advance = num2 + num;
				glyph.channel = bMGlyph.channel;
				if (fontScale != 1f)
				{
					GlyphInfo glyphInfo = glyph;
					glyphInfo.v0 *= fontScale;
					GlyphInfo glyphInfo2 = glyph;
					glyphInfo2.v1 *= fontScale;
					glyph.advance *= fontScale;
				}
				return glyph;
			}
		}
		else if ((Object)(object)dynamicFont != (Object)null && dynamicFont.GetCharacterInfo((char)ch, ref mTempChar, size, fontStyle))
		{
			glyph.v0.x = ((CharacterInfo)(ref mTempChar)).minX;
			glyph.v1.x = ((CharacterInfo)(ref mTempChar)).maxX;
			glyph.v0.y = (float)((CharacterInfo)(ref mTempChar)).maxY - baseline;
			glyph.v1.y = (float)((CharacterInfo)(ref mTempChar)).minY - baseline;
			glyph.u0 = ((CharacterInfo)(ref mTempChar)).uvTopLeft;
			glyph.u1 = ((CharacterInfo)(ref mTempChar)).uvBottomLeft;
			glyph.u2 = ((CharacterInfo)(ref mTempChar)).uvBottomRight;
			glyph.u3 = ((CharacterInfo)(ref mTempChar)).uvTopRight;
			glyph.advance = ((CharacterInfo)(ref mTempChar)).advance;
			glyph.channel = 0;
			glyph.v0.x = Mathf.Round(glyph.v0.x);
			glyph.v0.y = Mathf.Round(glyph.v0.y);
			glyph.v1.x = Mathf.Round(glyph.v1.x);
			glyph.v1.y = Mathf.Round(glyph.v1.y);
			float num3 = fontScale * pixelDensity;
			if (num3 != 1f)
			{
				GlyphInfo glyphInfo3 = glyph;
				glyphInfo3.v0 *= num3;
				GlyphInfo glyphInfo4 = glyph;
				glyphInfo4.v1 *= num3;
				glyph.advance *= num3;
			}
			return glyph;
		}
		return null;
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static float ParseAlpha(string text, int index)
	{
		int num = (NGUIMath.HexToDecimal(text[index + 1]) << 4) | NGUIMath.HexToDecimal(text[index + 2]);
		return Mathf.Clamp01((float)num / 255f);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static Color ParseColor(string text, int offset = 0)
	{
		//IL_0002: Unknown result type (might be due to invalid IL or missing references)
		return ParseColor24(text, offset);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static Color ParseColor24(string text, int offset = 0)
	{
		//IL_0070: Unknown result type (might be due to invalid IL or missing references)
		int num = (NGUIMath.HexToDecimal(text[offset]) << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
		int num2 = (NGUIMath.HexToDecimal(text[offset + 2]) << 4) | NGUIMath.HexToDecimal(text[offset + 3]);
		int num3 = (NGUIMath.HexToDecimal(text[offset + 4]) << 4) | NGUIMath.HexToDecimal(text[offset + 5]);
		float num4 = 0.003921569f;
		return new Color(num4 * (float)num, num4 * (float)num2, num4 * (float)num3);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static Color ParseColor32(string text, int offset)
	{
		//IL_0099: Unknown result type (might be due to invalid IL or missing references)
		int num = (NGUIMath.HexToDecimal(text[offset]) << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
		int num2 = (NGUIMath.HexToDecimal(text[offset + 2]) << 4) | NGUIMath.HexToDecimal(text[offset + 3]);
		int num3 = (NGUIMath.HexToDecimal(text[offset + 4]) << 4) | NGUIMath.HexToDecimal(text[offset + 5]);
		int num4 = (NGUIMath.HexToDecimal(text[offset + 6]) << 4) | NGUIMath.HexToDecimal(text[offset + 7]);
		float num5 = 0.003921569f;
		return new Color(num5 * (float)num, num5 * (float)num2, num5 * (float)num3, num5 * (float)num4);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static string EncodeColor(Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		return EncodeColor24(c);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static string EncodeColor(string text, Color c)
	{
		//IL_0010: Unknown result type (might be due to invalid IL or missing references)
		return "[c][" + EncodeColor24(c) + "]" + text + "[-][/c]";
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static string EncodeAlpha(float a)
	{
		int num = Mathf.Clamp(Mathf.RoundToInt(a * 255f), 0, 255);
		return NGUIMath.DecimalToHex8(num);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static string EncodeColor24(Color c)
	{
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		int num = 0xFFFFFF & (NGUIMath.ColorToInt(c) >> 8);
		return NGUIMath.DecimalToHex24(num);
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	public static string EncodeColor32(Color c)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		int num = NGUIMath.ColorToInt(c);
		return NGUIMath.DecimalToHex32(num);
	}

	public static bool ParseSymbol(string text, ref int index)
	{
		int sub = 1;
		bool bold = false;
		bool italic = false;
		bool underline = false;
		bool strike = false;
		bool ignoreColor = false;
		return ParseSymbol(text, ref index, null, premultiply: false, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static bool IsHex(char ch)
	{
		return (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');
	}

	public static bool ParseSpace(string text, int fontSize, ref int index, out int space)
	{
		if (!string.IsNullOrEmpty(text))
		{
			int length = text.Length;
			if (text[index] == '[' && index + 3 < length && text[index + 1] == '_')
			{
				int num = text.IndexOf(']', index + 1);
				if (num != -1)
				{
					bool flag = false;
					int num2 = 2;
					if (text[index + num2] == 'x')
					{
						flag = true;
						num2 = 3;
					}
					string s = text.Substring(index + num2, num - (index + num2));
					float result = 0f;
					if (float.TryParse(s, out result))
					{
						if (flag)
						{
							space = (int)((float)fontSize * result * fontScale);
						}
						else
						{
							space = (int)(result * fontScale);
						}
						index = num;
						return true;
					}
				}
			}
		}
		space = 0;
		return false;
	}

	public static bool ParseSize(string text, ref int index, ref int size)
	{
		if (!string.IsNullOrEmpty(text) && text[index] == '[')
		{
			int length = text.Length;
			if (index + 7 > length)
			{
				return false;
			}
			if (fontSizeStack.Count > 0 && text[index + 1] == '/' && text[index + 2] == 's' && text[index + 3] == 'i' && text[index + 4] == 'z' && text[index + 5] == 'e' && text[index + 6] == ']')
			{
				size = fontSizeStack.Pop();
				index += 7;
				return true;
			}
			if (text[index + 1] == 's' && text[index + 2] == 'i' && text[index + 3] == 'z' && text[index + 4] == 'e' && text[index + 5] == '=')
			{
				int num = text.IndexOf(']', index + 6);
				if (num != -1)
				{
					string s = text.Substring(index + 6, num - (index + 6));
					if (int.TryParse(s, out var result))
					{
						fontSizeStack.Push(size);
						size = result;
						index = num + 1;
						return true;
					}
				}
			}
		}
		return false;
	}

	public static bool ParseSymbol(string text, ref int index, BetterList<Color> colors, bool premultiply, ref int sub, ref bool bold, ref bool italic, ref bool underline, ref bool strike, ref bool ignoreColor)
	{
		//IL_048f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0494: Unknown result type (might be due to invalid IL or missing references)
		//IL_052c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0531: Unknown result type (might be due to invalid IL or missing references)
		//IL_04b9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04be: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f9: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f1: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0578: Unknown result type (might be due to invalid IL or missing references)
		//IL_0562: Unknown result type (might be due to invalid IL or missing references)
		//IL_0567: Unknown result type (might be due to invalid IL or missing references)
		//IL_0570: Unknown result type (might be due to invalid IL or missing references)
		//IL_0575: Unknown result type (might be due to invalid IL or missing references)
		int length = text.Length;
		if (index + 3 > length || text[index] != '[')
		{
			return false;
		}
		if (text[index + 2] == ']')
		{
			if (text[index + 1] == '-')
			{
				if (colors != null && colors.size > 1)
				{
					colors.RemoveAt(colors.size - 1);
				}
				index += 3;
				return true;
			}
			switch (text.Substring(index, 3))
			{
			case "[b]":
				bold = true;
				index += 3;
				return true;
			case "[i]":
				italic = true;
				index += 3;
				return true;
			case "[u]":
				underline = true;
				index += 3;
				return true;
			case "[s]":
				strike = true;
				index += 3;
				return true;
			case "[c]":
				ignoreColor = true;
				index += 3;
				return true;
			}
		}
		if (index + 4 > length)
		{
			return false;
		}
		if (text[index + 3] == ']')
		{
			switch (text.Substring(index, 4))
			{
			case "[/b]":
				bold = false;
				index += 4;
				return true;
			case "[/i]":
				italic = false;
				index += 4;
				return true;
			case "[/u]":
				underline = false;
				index += 4;
				return true;
			case "[/s]":
				strike = false;
				index += 4;
				return true;
			case "[/c]":
				ignoreColor = false;
				index += 4;
				return true;
			}
			char ch = text[index + 1];
			char ch2 = text[index + 2];
			if (IsHex(ch) && IsHex(ch2))
			{
				int num = (NGUIMath.HexToDecimal(ch) << 4) | NGUIMath.HexToDecimal(ch2);
				mAlpha = (float)num / 255f;
				index += 4;
				return true;
			}
		}
		if (index + 5 > length)
		{
			return false;
		}
		if (text[index + 4] == ']')
		{
			switch (text.Substring(index, 5))
			{
			case "[sub]":
				sub = 1;
				index += 5;
				return true;
			case "[sup]":
				sub = 2;
				index += 5;
				return true;
			}
		}
		if (index + 6 > length)
		{
			return false;
		}
		if (text[index + 5] == ']')
		{
			switch (text.Substring(index, 6))
			{
			case "[/sub]":
				sub = 0;
				index += 6;
				return true;
			case "[/sup]":
				sub = 0;
				index += 6;
				return true;
			case "[/url]":
				index += 6;
				return true;
			}
		}
		if (text[index + 1] == 'u' && text[index + 2] == 'r' && text[index + 3] == 'l' && text[index + 4] == '=')
		{
			int num2 = text.IndexOf(']', index + 4);
			if (num2 != -1)
			{
				index = num2 + 1;
				return true;
			}
			index = text.Length;
			return true;
		}
		if (index + 8 > length)
		{
			return false;
		}
		if (text[index + 7] == ']')
		{
			Color val = ParseColor24(text, index + 1);
			if (!IsColorEncoded(text, index + 1, 6))
			{
				return false;
			}
			if (colors != null)
			{
				val.a = colors[colors.size - 1].a;
				if (premultiply && val.a != 1f)
				{
					val = Color.Lerp(mInvisible, val, val.a);
				}
				colors.Add(val);
			}
			index += 8;
			return true;
		}
		if (index + 10 > length)
		{
			return false;
		}
		if (text[index + 9] == ']')
		{
			Color val2 = ParseColor32(text, index + 1);
			if (!IsColorEncoded(text, index + 1, 8))
			{
				return false;
			}
			if (colors != null)
			{
				if (premultiply && val2.a != 1f)
				{
					val2 = Color.Lerp(mInvisible, val2, val2.a);
				}
				colors.Add(val2);
			}
			index += 10;
			return true;
		}
		return false;
	}

	private static bool IsColorEncoded(string text, int index, int length)
	{
		for (int i = 0; i < length; i++)
		{
			char c = text[i + index];
			if ((c < 'a' || c > 'f') && (c < 'A' || c > 'F') && (c < '0' || c > '9'))
			{
				return false;
			}
		}
		return true;
	}

	public static string StripSymbols(string text)
	{
		if (text != null)
		{
			int num = 0;
			int length = text.Length;
			while (num < length)
			{
				char c = text[num];
				if (c == '[')
				{
					int sub = 0;
					bool bold = false;
					bool italic = false;
					bool underline = false;
					bool strike = false;
					bool ignoreColor = false;
					int index = num;
					if (ParseSymbol(text, ref index, null, premultiply: false, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor))
					{
						text = text.Remove(num, index - num);
						length = text.Length;
						continue;
					}
					int size = 0;
					if (ParseSize(text, ref index, ref size))
					{
						text = text.Remove(num, index - num);
						length = text.Length;
						continue;
					}
				}
				num++;
			}
		}
		return text;
	}

	public static void Align(BetterList<Vector3> verts, int indexOffset, float printedWidth, int elements = 4)
	{
		switch (alignment)
		{
		case Alignment.Right:
		{
			float num16 = (float)rectWidth - printedWidth;
			if (!(num16 < 0f))
			{
				for (int j = indexOffset; j < verts.size; j++)
				{
					ref Vector3 reference2 = ref verts.buffer[j];
					reference2.x += num16;
				}
			}
			break;
		}
		case Alignment.Center:
		{
			float num13 = ((float)rectWidth - printedWidth) * 0.5f;
			if (!(num13 < 0f))
			{
				int num14 = Mathf.RoundToInt((float)rectWidth - printedWidth);
				int num15 = Mathf.RoundToInt((float)rectWidth);
				bool flag = (num14 & 1) == 1;
				bool flag2 = (num15 & 1) == 1;
				if ((flag && !flag2) || (!flag && flag2))
				{
					num13 += 0.5f * fontScale;
				}
				for (int i = indexOffset; i < verts.size; i++)
				{
					ref Vector3 reference = ref verts.buffer[i];
					reference.x += num13;
				}
			}
			break;
		}
		case Alignment.Justified:
		{
			if (printedWidth < (float)rectWidth * 0.65f)
			{
				break;
			}
			float num = ((float)rectWidth - printedWidth) * 0.5f;
			if (num < 1f)
			{
				break;
			}
			int num2 = (verts.size - indexOffset) / elements;
			if (num2 < 1)
			{
				break;
			}
			float num3 = 1f / (float)(num2 - 1);
			float num4 = (float)rectWidth / printedWidth;
			int num5 = indexOffset + elements;
			int num6 = 1;
			while (num5 < verts.size)
			{
				float x = verts.buffer[num5].x;
				float x2 = verts.buffer[num5 + elements / 2].x;
				float num7 = x2 - x;
				float num8 = x * num4;
				float num9 = num8 + num7;
				float num10 = x2 * num4;
				float num11 = num10 - num7;
				float num12 = (float)num6 * num3;
				x2 = Mathf.Lerp(num9, num10, num12);
				x = Mathf.Lerp(num8, num11, num12);
				x = Mathf.Round(x);
				x2 = Mathf.Round(x2);
				switch (elements)
				{
				case 4:
					verts.buffer[num5++].x = x;
					verts.buffer[num5++].x = x;
					verts.buffer[num5++].x = x2;
					verts.buffer[num5++].x = x2;
					break;
				case 2:
					verts.buffer[num5++].x = x;
					verts.buffer[num5++].x = x2;
					break;
				case 1:
					verts.buffer[num5++].x = x;
					break;
				}
				num6++;
			}
			break;
		}
		}
	}

	public static int GetExactCharacterIndex(BetterList<Vector3> verts, BetterList<int> indices, Vector2 pos)
	{
		//IL_0011: Unknown result type (might be due to invalid IL or missing references)
		//IL_0016: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0039: Unknown result type (might be due to invalid IL or missing references)
		//IL_0059: Unknown result type (might be due to invalid IL or missing references)
		//IL_005e: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		for (int i = 0; i < indices.size; i++)
		{
			int num = i << 1;
			int i2 = num + 1;
			float x = verts[num].x;
			if (pos.x < x)
			{
				continue;
			}
			float x2 = verts[i2].x;
			if (pos.x > x2)
			{
				continue;
			}
			float y = verts[num].y;
			if (!(pos.y < y))
			{
				float y2 = verts[i2].y;
				if (!(pos.y > y2))
				{
					return indices[i];
				}
			}
		}
		return 0;
	}

	public static int GetApproximateCharacterIndex(BetterList<Vector3> verts, BetterList<int> indices, Vector2 pos)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		//IL_004a: Unknown result type (might be due to invalid IL or missing references)
		//IL_004f: Unknown result type (might be due to invalid IL or missing references)
		float num = float.MaxValue;
		float num2 = float.MaxValue;
		int i = 0;
		for (int j = 0; j < verts.size; j++)
		{
			float num3 = Mathf.Abs(pos.y - verts[j].y);
			if (!(num3 > num2))
			{
				float num4 = Mathf.Abs(pos.x - verts[j].x);
				if (num3 < num2)
				{
					num2 = num3;
					num = num4;
					i = j;
				}
				else if (num4 < num)
				{
					num = num4;
					i = j;
				}
			}
		}
		return indices[i];
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static bool IsSpace(int ch)
	{
		return ch == 32 || ch == 8202 || ch == 8203 || ch == 8201;
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static void EndLine(ref StringBuilder s)
	{
		int num = s.Length - 1;
		if (num > 0 && IsSpace(s[num]))
		{
			s[num] = '\n';
		}
		else
		{
			s.Append('\n');
		}
	}

	[DebuggerStepThrough]
	[DebuggerHidden]
	private static void ReplaceSpaceWithNewline(ref StringBuilder s)
	{
		int num = s.Length - 1;
		if (num > 0 && IsSpace(s[num]))
		{
			s[num] = '\n';
		}
	}

	public static Vector2 CalculatePrintedSize(string text)
	{
		//IL_0000: Unknown result type (might be due to invalid IL or missing references)
		//IL_0005: Unknown result type (might be due to invalid IL or missing references)
		//IL_02bf: Unknown result type (might be due to invalid IL or missing references)
		Vector2 zero = Vector2.zero;
		if (!string.IsNullOrEmpty(text))
		{
			Prepare(text);
			float num = 0f;
			float num2 = 0f;
			float num3 = 0f;
			int length = text.Length;
			int num4 = 0;
			int prev = 0;
			int size = finalSize;
			MaxLineHeight maxLineHeight = new MaxLineHeight(finalLineHeight);
			float val = maxLineHeight.Get();
			float num5 = 0f;
			for (int i = 0; i < length; i++)
			{
				num4 = text[i];
				if (num4 == 10)
				{
					if (num > num3)
					{
						num3 = num;
					}
					num = 0f;
					num2 += maxLineHeight;
					maxLineHeight.Reset();
				}
				else
				{
					if (num4 < 32)
					{
						continue;
					}
					if (encoding && ParseSymbol(text, ref i))
					{
						i--;
						continue;
					}
					if (encoding)
					{
						if (num5 < num2)
						{
							maxLineHeight.Reset();
							num5 = num2;
						}
						if (ParseSize(text, ref i, ref size))
						{
							val = ((float)size + spacingY) * fontScale;
							i--;
							continue;
						}
					}
					maxLineHeight.Set(val);
					if (encoding && ParseSpace(text, size, ref i, out var space))
					{
						float num6 = space;
						num6 += finalSpacingX;
						if (Mathf.RoundToInt(num + num6) > regionWidth)
						{
							if (num > num3)
							{
								num3 = num - finalSpacingX;
							}
							num = num6;
							num2 += maxLineHeight;
						}
						else
						{
							num += num6;
						}
						prev = 0;
						continue;
					}
					BMSymbol bMSymbol = ((!useSymbols) ? null : GetSymbol(text, i, length));
					if (bMSymbol == null)
					{
						float glyphWidth = GetGlyphWidth(num4, prev, size);
						if (glyphWidth == 0f)
						{
							continue;
						}
						glyphWidth += finalSpacingX;
						if (Mathf.RoundToInt(num + glyphWidth) > regionWidth)
						{
							if (num > num3)
							{
								num3 = num - finalSpacingX;
							}
							num = glyphWidth;
							num2 += maxLineHeight;
						}
						else
						{
							num += glyphWidth;
						}
						prev = num4;
						continue;
					}
					float num7 = finalSpacingX + (float)bMSymbol.advance * fontScale;
					if (Mathf.RoundToInt(num + num7) > regionWidth)
					{
						if (num > num3)
						{
							num3 = num - finalSpacingX;
						}
						num = num7;
						num2 += maxLineHeight;
					}
					else
					{
						num += num7;
					}
					i += bMSymbol.sequence.Length - 1;
					prev = 0;
				}
			}
			zero.x = ((!(num > num3)) ? num3 : (num - finalSpacingX));
			zero.y = num2 + maxLineHeight;
		}
		else
		{
			zero.y = (float)fontSize * fontScale;
		}
		return zero;
	}

	public static int CalculateOffsetToFit(string text)
	{
		if (string.IsNullOrEmpty(text) || regionWidth < 1)
		{
			return 0;
		}
		Prepare(text);
		int length = text.Length;
		int num = 0;
		int prev = 0;
		int i = 0;
		for (int length2 = text.Length; i < length2; i++)
		{
			BMSymbol bMSymbol = ((!useSymbols) ? null : GetSymbol(text, i, length));
			if (bMSymbol == null)
			{
				num = text[i];
				float glyphWidth = GetGlyphWidth(num, prev);
				if (glyphWidth != 0f)
				{
					mSizes.Add(finalSpacingX + glyphWidth);
				}
				prev = num;
				continue;
			}
			mSizes.Add(finalSpacingX + (float)bMSymbol.advance * fontScale);
			int j = 0;
			for (int num2 = bMSymbol.sequence.Length - 1; j < num2; j++)
			{
				mSizes.Add(0f);
			}
			i += bMSymbol.sequence.Length - 1;
			prev = 0;
		}
		float num3 = regionWidth;
		int num4 = mSizes.size;
		while (num4 > 0 && num3 > 0f)
		{
			num3 -= mSizes[--num4];
		}
		mSizes.Clear();
		if (num3 < 0f)
		{
			num4++;
		}
		return num4;
	}

	public static string GetEndOfLineThatFits(string text)
	{
		int length = text.Length;
		int num = CalculateOffsetToFit(text);
		return text.Substring(num, length - num);
	}

	public static bool WrapText(string text, out string finalText, bool wrapLineColors = false)
	{
		return WrapText(text, out finalText, keepCharCount: false, wrapLineColors);
	}

	public static bool WrapText(string text, out string finalText, bool keepCharCount, bool wrapLineColors, bool useEllipsis = false)
	{
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0358: Unknown result type (might be due to invalid IL or missing references)
		//IL_036e: Unknown result type (might be due to invalid IL or missing references)
		//IL_052d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Unknown result type (might be due to invalid IL or missing references)
		//IL_0548: Unknown result type (might be due to invalid IL or missing references)
		//IL_054d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0503: Unknown result type (might be due to invalid IL or missing references)
		//IL_0508: Unknown result type (might be due to invalid IL or missing references)
		//IL_0587: Unknown result type (might be due to invalid IL or missing references)
		//IL_058c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0450: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a50: Unknown result type (might be due to invalid IL or missing references)
		//IL_094a: Unknown result type (might be due to invalid IL or missing references)
		if (regionWidth < 1 || regionHeight < 1 || finalLineHeight < 1f)
		{
			finalText = string.Empty;
			return false;
		}
		int size = finalSize;
		fontSizeStack.Clear();
		float num = regionWidth;
		float num2 = regionHeight;
		MaxLineHeight maxLineHeight = new MaxLineHeight(finalLineHeight);
		int num3 = 0;
		int i = 0;
		int num4 = 1;
		int prev = 0;
		bool flag = false;
		int length = text.Length;
		bool flag2 = false;
		int num5 = ((maxLines <= 0) ? 1000000 : maxLines);
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		if (encoding)
		{
			num4 = 0;
			for (; i < length; i++)
			{
				char c = text[i];
				if (c == '\n')
				{
					num = regionWidth;
					flag = false;
					if (num4 + 1 == num5)
					{
						break;
					}
					num2 -= maxLineHeight.Get();
					if (num2 < 0f)
					{
						break;
					}
					num4++;
					continue;
				}
				if (ParseSymbol(text, ref i))
				{
					i--;
					continue;
				}
				if (ParseSize(text, ref i, ref size))
				{
					i--;
					continue;
				}
				maxLineHeight.Set(((float)size + spacingY) * fontScale);
				BMSymbol bMSymbol = ((!useSymbols) ? null : GetSymbol(text, i, length));
				int space = 0;
				float num6;
				if (encoding && ParseSpace(text, size, ref i, out space))
				{
					num6 = space;
				}
				else if (bMSymbol == null)
				{
					float glyphWidth = GetGlyphWidth(c, prev, size);
					if (glyphWidth == 0f && !IsSpace(c))
					{
						continue;
					}
					num6 = finalSpacingX + glyphWidth;
				}
				else
				{
					num6 = finalSpacingX + (float)bMSymbol.advance * fontScale;
				}
				num -= num6;
				if (IsSpace(c) && !flag2 && num3 < i)
				{
					num3 = i + 1;
					flag = true;
				}
				if (Mathf.RoundToInt(num) < 0)
				{
					num = regionWidth;
					i = ((!flag) ? i : (num3 - 1));
					flag = false;
					prev = 0;
					if (num4 + 1 == num5)
					{
						break;
					}
					num2 -= maxLineHeight;
					if (num2 < 0f)
					{
						break;
					}
					num4++;
					maxLineHeight.Reset();
				}
				else if (bMSymbol != null)
				{
					i += bMSymbol.length - 1;
				}
			}
			if (num2 - maxLineHeight >= 0f)
			{
				num4++;
			}
			num5 = num4;
		}
		else
		{
			float num7 = ((maxLines <= 0) ? ((float)regionHeight) : Mathf.Min((float)regionHeight, finalLineHeight * (float)maxLines));
			num5 = Mathf.FloorToInt(Mathf.Min((float)num5, num7 / finalLineHeight) + 0.01f);
		}
		if (num5 == 0)
		{
			finalText = string.Empty;
			return false;
		}
		StringBuilder s = new StringBuilder();
		bool flag3 = true;
		bool flag4 = true;
		Color val = tint;
		int sub = 0;
		bool bold = false;
		bool italic = false;
		bool underline = false;
		bool strike = false;
		bool ignoreColor = false;
		if (!useSymbols)
		{
			wrapLineColors = false;
		}
		if (wrapLineColors)
		{
			mColors.Add(val);
			s.Append("[");
			s.Append(EncodeColor(val));
			s.Append("]");
		}
		num3 = 0;
		i = 0;
		num4 = 1;
		prev = 0;
		num = regionWidth;
		for (; i < length; i++)
		{
			char c2 = text[i];
			if (c2 == '\n')
			{
				if (num4 == num5)
				{
					break;
				}
				num = regionWidth;
				if (num3 < i)
				{
					s.Append(text.Substring(num3, i - num3 + 1));
				}
				else
				{
					s.Append(c2);
				}
				if (wrapLineColors)
				{
					for (int j = 0; j < mColors.size; j++)
					{
						s.Insert(s.Length - 1, "[-]");
					}
					for (int k = 0; k < mColors.size; k++)
					{
						s.Append("[");
						s.Append(EncodeColor(mColors[k]));
						s.Append("]");
					}
				}
				flag3 = true;
				num4++;
				num3 = i + 1;
				prev = 0;
				continue;
			}
			if (encoding)
			{
				if (!wrapLineColors)
				{
					if (ParseSymbol(text, ref i))
					{
						i--;
						continue;
					}
				}
				else if (ParseSymbol(text, ref i, mColors, premultiply, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor))
				{
					if (ignoreColor)
					{
						val = mColors[mColors.size - 1];
						val.a *= mAlpha * tint.a;
					}
					else
					{
						val = tint * mColors[mColors.size - 1];
						val.a *= mAlpha;
					}
					int l = 0;
					for (int num8 = mColors.size - 2; l < num8; l++)
					{
						val.a *= mColors[l].a;
					}
					i--;
					continue;
				}
				if (ParseSize(text, ref i, ref size))
				{
					i--;
					continue;
				}
			}
			BMSymbol bMSymbol2 = ((!useSymbols) ? null : GetSymbol(text, i, length));
			int space2 = 0;
			int num9 = i;
			float num10;
			if (encoding && ParseSpace(text, size, ref i, out space2))
			{
				num10 = space2;
			}
			else if (bMSymbol2 == null)
			{
				float glyphWidth2 = GetGlyphWidth(c2, prev, size);
				if (glyphWidth2 == 0f && !IsSpace(c2))
				{
					continue;
				}
				num10 = finalSpacingX + glyphWidth2;
			}
			else
			{
				num10 = finalSpacingX + (float)bMSymbol2.advance * fontScale;
			}
			num -= num10;
			if (IsSpace(c2) && !flag2 && num3 < i)
			{
				int num11 = i - num3 + 1;
				if (num4 == num5 && num <= 0f && i < length)
				{
					char c3 = text[i];
					if (c3 < ' ' || IsSpace(c3))
					{
						num11--;
					}
				}
				s.Append(text.Substring(num3, num11));
				flag3 = false;
				num3 = i + 1;
				prev = c2;
			}
			if (Mathf.RoundToInt(num) < 0)
			{
				if (!flag3 && num4 != num5)
				{
					flag3 = true;
					num = regionWidth;
					i = num3 - 1;
					prev = 0;
					if (num4++ == num5)
					{
						break;
					}
					if (keepCharCount)
					{
						ReplaceSpaceWithNewline(ref s);
					}
					else
					{
						EndLine(ref s);
					}
					if (wrapLineColors)
					{
						for (int m = 0; m < mColors.size; m++)
						{
							s.Insert(s.Length - 1, "[-]");
						}
						for (int n = 0; n < mColors.size; n++)
						{
							s.Append("[");
							s.Append(EncodeColor(mColors[n]));
							s.Append("]");
						}
					}
					continue;
				}
				if (useEllipsis && num4 == num5 && i > 1)
				{
					float num12 = GetGlyphWidth(46, 46) * 3f;
					if (num12 < (float)regionWidth)
					{
						num += num10;
						int num13 = i;
						int num14 = 0;
						while (num13 > 1 && num < num12)
						{
							num13--;
							char prev2 = text[num13 - 1];
							char ch = text[num13];
							bool flag5 = num == 0f && IsSpace(ch);
							num += GetGlyphWidth(ch, prev2);
							if (num13 < num3 && !flag5)
							{
								num14++;
							}
						}
						if (num >= num12)
						{
							if (num14 > 0)
							{
								s.Length = Mathf.Max(0, s.Length - num14);
							}
							s.Append(text.Substring(num3, Mathf.Max(0, num13 - num3)));
							while (s.Length > 0 && IsSpace(s[s.Length - 1]))
							{
								s.Length--;
							}
							s.Append("...");
							num4++;
							num3 = (i = num13);
							break;
						}
					}
				}
				s.Append(text.Substring(num3, Mathf.Max(0, ((space2 <= 0) ? i : num9) - num3)));
				bool flag6 = IsSpace(c2);
				if (!flag6 && !flag2)
				{
					flag4 = false;
				}
				if (wrapLineColors && mColors.size > 0)
				{
					s.Append("[-]");
				}
				if (num4++ == num5)
				{
					num3 = i;
					break;
				}
				if (keepCharCount)
				{
					ReplaceSpaceWithNewline(ref s);
				}
				else
				{
					EndLine(ref s);
				}
				if (wrapLineColors)
				{
					for (int num15 = 0; num15 < mColors.size; num15++)
					{
						s.Insert(s.Length - 1, "[-]");
					}
					for (int num16 = 0; num16 < mColors.size; num16++)
					{
						s.Append("[");
						s.Append(EncodeColor(mColors[num16]));
						s.Append("]");
					}
				}
				flag3 = true;
				if (flag6 || space2 > 0)
				{
					num3 = i + 1;
					num = regionWidth;
				}
				else
				{
					num3 = i;
					num = (float)regionWidth - num10;
				}
				prev = 0;
			}
			else
			{
				prev = c2;
			}
			if (bMSymbol2 != null)
			{
				i += bMSymbol2.length - 1;
				prev = 0;
			}
		}
		if (num3 < i)
		{
			s.Append(text.Substring(num3, i - num3));
		}
		if (wrapLineColors && mColors.size > 0)
		{
			s.Append("[-]");
		}
		finalText = s.ToString();
		mColors.Clear();
		return flag4 && (i == length || num4 <= Mathf.Min(maxLines, num5));
	}

	public static void Print(string text, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0052: Unknown result type (might be due to invalid IL or missing references)
		//IL_0057: Unknown result type (might be due to invalid IL or missing references)
		//IL_005c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0061: Unknown result type (might be due to invalid IL or missing references)
		//IL_0066: Unknown result type (might be due to invalid IL or missing references)
		//IL_0068: Unknown result type (might be due to invalid IL or missing references)
		//IL_006d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0072: Unknown result type (might be due to invalid IL or missing references)
		//IL_0077: Unknown result type (might be due to invalid IL or missing references)
		//IL_007c: Unknown result type (might be due to invalid IL or missing references)
		//IL_007e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0083: Unknown result type (might be due to invalid IL or missing references)
		//IL_0085: Unknown result type (might be due to invalid IL or missing references)
		//IL_0087: Unknown result type (might be due to invalid IL or missing references)
		//IL_008c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0098: Unknown result type (might be due to invalid IL or missing references)
		//IL_0127: Unknown result type (might be due to invalid IL or missing references)
		//IL_012c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0301: Unknown result type (might be due to invalid IL or missing references)
		//IL_0307: Invalid comparison between Unknown and I4
		//IL_030c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0312: Invalid comparison between Unknown and I4
		//IL_031c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0322: Invalid comparison between Unknown and I4
		//IL_023e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0254: Unknown result type (might be due to invalid IL or missing references)
		//IL_0259: Unknown result type (might be due to invalid IL or missing references)
		//IL_025e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0214: Unknown result type (might be due to invalid IL or missing references)
		//IL_0219: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_032d: Invalid comparison between Unknown and I4
		//IL_0273: Unknown result type (might be due to invalid IL or missing references)
		//IL_0275: Unknown result type (might be due to invalid IL or missing references)
		//IL_027a: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ce: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_02d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_02da: Unknown result type (might be due to invalid IL or missing references)
		//IL_02df: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e1: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_02e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_02ed: Unknown result type (might be due to invalid IL or missing references)
		//IL_02f2: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e6: Unknown result type (might be due to invalid IL or missing references)
		//IL_04f5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0504: Unknown result type (might be due to invalid IL or missing references)
		//IL_0663: Unknown result type (might be due to invalid IL or missing references)
		//IL_0668: Unknown result type (might be due to invalid IL or missing references)
		//IL_067b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0680: Unknown result type (might be due to invalid IL or missing references)
		//IL_0687: Unknown result type (might be due to invalid IL or missing references)
		//IL_068c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0693: Unknown result type (might be due to invalid IL or missing references)
		//IL_0698: Unknown result type (might be due to invalid IL or missing references)
		//IL_069f: Unknown result type (might be due to invalid IL or missing references)
		//IL_06a4: Unknown result type (might be due to invalid IL or missing references)
		//IL_06ab: Unknown result type (might be due to invalid IL or missing references)
		//IL_06b0: Unknown result type (might be due to invalid IL or missing references)
		//IL_053d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0542: Unknown result type (might be due to invalid IL or missing references)
		//IL_056d: Unknown result type (might be due to invalid IL or missing references)
		//IL_057c: Unknown result type (might be due to invalid IL or missing references)
		//IL_058b: Unknown result type (might be due to invalid IL or missing references)
		//IL_059a: Unknown result type (might be due to invalid IL or missing references)
		//IL_05d8: Unknown result type (might be due to invalid IL or missing references)
		//IL_05dd: Unknown result type (might be due to invalid IL or missing references)
		//IL_05f6: Unknown result type (might be due to invalid IL or missing references)
		//IL_05be: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ce3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cf5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d07: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d19: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c8a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c99: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ca8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0cb7: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ae8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aea: Unknown result type (might be due to invalid IL or missing references)
		//IL_0aee: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af3: Unknown result type (might be due to invalid IL or missing references)
		//IL_0af8: Unknown result type (might be due to invalid IL or missing references)
		//IL_0afa: Unknown result type (might be due to invalid IL or missing references)
		//IL_0afe: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b03: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b95: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b97: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b99: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ba0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ba5: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c3e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c40: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c45: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a42: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a4f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a5c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0a69: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d7e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0d96: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dae: Unknown result type (might be due to invalid IL or missing references)
		//IL_0dc6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b7a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b21: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b2c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b37: Unknown result type (might be due to invalid IL or missing references)
		//IL_0b42: Unknown result type (might be due to invalid IL or missing references)
		//IL_0c61: Unknown result type (might be due to invalid IL or missing references)
		//IL_105a: Unknown result type (might be due to invalid IL or missing references)
		//IL_1069: Unknown result type (might be due to invalid IL or missing references)
		//IL_1077: Unknown result type (might be due to invalid IL or missing references)
		//IL_1085: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d5: Unknown result type (might be due to invalid IL or missing references)
		//IL_10d7: Unknown result type (might be due to invalid IL or missing references)
		//IL_10db: Unknown result type (might be due to invalid IL or missing references)
		//IL_10e0: Unknown result type (might be due to invalid IL or missing references)
		//IL_10e5: Unknown result type (might be due to invalid IL or missing references)
		//IL_10e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_10eb: Unknown result type (might be due to invalid IL or missing references)
		//IL_10f0: Unknown result type (might be due to invalid IL or missing references)
		//IL_0ffb: Unknown result type (might be due to invalid IL or missing references)
		//IL_1010: Unknown result type (might be due to invalid IL or missing references)
		//IL_1024: Unknown result type (might be due to invalid IL or missing references)
		//IL_1038: Unknown result type (might be due to invalid IL or missing references)
		//IL_0efc: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f15: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f2e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0f47: Unknown result type (might be due to invalid IL or missing references)
		//IL_1167: Unknown result type (might be due to invalid IL or missing references)
		//IL_110e: Unknown result type (might be due to invalid IL or missing references)
		//IL_1119: Unknown result type (might be due to invalid IL or missing references)
		//IL_1124: Unknown result type (might be due to invalid IL or missing references)
		//IL_112f: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(text))
		{
			return;
		}
		int size = verts.size;
		Prepare(text);
		mColors.Add(Color.white);
		mAlpha = 1f;
		int num = 0;
		int prev = 0;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = finalSize;
		Color val = (tint * gradientBottom).GammaToLinearSpace();
		Color val2 = (tint * gradientTop).GammaToLinearSpace();
		Color val3 = tint;
		Color item = val3.GammaToLinearSpace();
		int length = text.Length;
		Rect val4 = default(Rect);
		float num6 = 0f;
		float num7 = 0f;
		float num8 = num5 * pixelDensity;
		bool flag = false;
		int sub = 0;
		bool flag2 = false;
		bool bold = false;
		bool italic = false;
		bool underline = false;
		bool strike = false;
		bool ignoreColor = false;
		int size2 = finalSize;
		fontSizeStack.Clear();
		MaxLineHeight maxLineHeight = new MaxLineHeight((float)size2 * fontScale);
		float val5 = maxLineHeight.Get() + spacingY;
		float num9 = 0f;
		int start = size;
		int num10 = 0;
		float num11 = 0f;
		if ((Object)(object)bitmapFont != (Object)null)
		{
			val4 = bitmapFont.uvRect;
			num6 = ((Rect)(ref val4)).width / (float)bitmapFont.texWidth;
			num7 = ((Rect)(ref val4)).height / (float)bitmapFont.texHeight;
		}
		for (int i = 0; i < length; i++)
		{
			num = text[i];
			num11 = num2;
			if (num == 10)
			{
				if (num2 > num4)
				{
					num4 = num2;
				}
				if (alignment != Alignment.Left)
				{
					Align(verts, size, num2 - finalSpacingX);
					size = verts.size;
				}
				num2 = 0f;
				num3 += maxLineHeight;
				prev = 0;
				num10 = verts.size;
				continue;
			}
			if (num < 32)
			{
				prev = num;
				continue;
			}
			if (encoding && ParseSymbol(text, ref i, mColors, premultiply, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor))
			{
				if (ignoreColor)
				{
					val3 = mColors[mColors.size - 1];
					val3.a *= mAlpha * tint.a;
				}
				else
				{
					val3 = tint * mColors[mColors.size - 1];
					val3.a *= mAlpha;
				}
				item = val3.GammaToLinearSpace();
				int j = 0;
				for (int num12 = mColors.size - 2; j < num12; j++)
				{
					val3.a *= mColors[j].a;
				}
				if (gradient)
				{
					val = (gradientBottom * val3).GammaToLinearSpace();
					val2 = (gradientTop * val3).GammaToLinearSpace();
				}
				i--;
				continue;
			}
			bold |= (int)_fontStyle == 1 || (int)_fontStyle == 3;
			italic |= (int)_fontStyle == 2 || (int)_fontStyle == 3;
			int space = 0;
			if (encoding && ParseSpace(text, size2, ref i, out space))
			{
				flag2 = false;
				num = 32;
			}
			else
			{
				flag2 = bold;
			}
			if (encoding)
			{
				if (num9 < num3)
				{
					ChangeBaseline(verts, start, num10, maxLineHeight.Get());
					start = num10;
					maxLineHeight.Reset();
					num9 = num3;
				}
				if (ParseSize(text, ref i, ref size2))
				{
					val5 = (float)size2 * fontScale + spacingY;
					i--;
					continue;
				}
			}
			maxLineHeight.Set(val5);
			BMSymbol bMSymbol = ((!useSymbols) ? null : GetSymbol(text, i, length));
			float num13;
			float num14;
			float num16;
			float num15;
			if (bMSymbol != null)
			{
				num13 = num2 + (float)bMSymbol.offsetX * fontScale;
				num14 = num13 + (float)bMSymbol.width * fontScale;
				num15 = 0f - (num3 + (float)bMSymbol.offsetY * fontScale);
				num16 = num15 - (float)bMSymbol.height * fontScale;
				if (Mathf.RoundToInt(num2 + (float)bMSymbol.advance * fontScale) > regionWidth)
				{
					if (num2 == 0f)
					{
						return;
					}
					if (alignment != Alignment.Left && size < verts.size)
					{
						Align(verts, size, num2 - finalSpacingX);
						size = verts.size;
					}
					num13 -= num2;
					num14 -= num2;
					num16 -= maxLineHeight;
					num15 -= maxLineHeight;
					num2 = 0f;
					num3 += maxLineHeight;
					num11 = 0f;
					num10 = verts.size;
				}
				verts.Add(new Vector3(num13, num16));
				verts.Add(new Vector3(num13, num15));
				verts.Add(new Vector3(num14, num15));
				verts.Add(new Vector3(num14, num16));
				num2 += finalSpacingX + (float)bMSymbol.advance * fontScale;
				i += bMSymbol.length - 1;
				prev = 0;
				if (uvs != null)
				{
					Rect uvRect = bMSymbol.uvRect;
					float xMin = ((Rect)(ref uvRect)).xMin;
					float yMin = ((Rect)(ref uvRect)).yMin;
					float xMax = ((Rect)(ref uvRect)).xMax;
					float yMax = ((Rect)(ref uvRect)).yMax;
					uvs.Add(new Vector2(xMin, yMin));
					uvs.Add(new Vector2(xMin, yMax));
					uvs.Add(new Vector2(xMax, yMax));
					uvs.Add(new Vector2(xMax, yMin));
				}
				if (cols == null)
				{
					continue;
				}
				if (symbolStyle == SymbolStyle.Colored)
				{
					for (int k = 0; k < 4; k++)
					{
						cols.Add(item);
					}
					continue;
				}
				Color white = Color.white;
				white.a = item.a;
				for (int l = 0; l < 4; l++)
				{
					cols.Add(white);
				}
				continue;
			}
			GlyphInfo glyphInfo = GetGlyph(num, prev, size2);
			if (glyphInfo == null)
			{
				continue;
			}
			prev = num;
			if (space > 0)
			{
				float num17 = baseline + ((float)finalSize - baseline) * (float)size2 / (float)finalSize;
				glyphInfo.v0 = new Vector2(0f, ((float)size2 - num17) * fontScale);
				glyphInfo.v1 = new Vector2((float)space, (0f - num17) * fontScale);
				glyphInfo.u0 = Vector2.zero;
				glyphInfo.u1 = Vector2.zero;
				glyphInfo.u2 = Vector2.zero;
				glyphInfo.u3 = Vector2.zero;
				glyphInfo.advance = space;
			}
			if (sub != 0)
			{
				ref Vector2 v = ref glyphInfo.v0;
				v.x *= 0.75f;
				ref Vector2 v2 = ref glyphInfo.v0;
				v2.y *= 0.75f;
				ref Vector2 v3 = ref glyphInfo.v1;
				v3.x *= 0.75f;
				ref Vector2 v4 = ref glyphInfo.v1;
				v4.y *= 0.75f;
				if (sub == 1)
				{
					ref Vector2 v5 = ref glyphInfo.v0;
					v5.y -= fontScale * (float)size2 * 0.4f;
					ref Vector2 v6 = ref glyphInfo.v1;
					v6.y -= fontScale * (float)size2 * 0.4f;
				}
				else
				{
					ref Vector2 v7 = ref glyphInfo.v0;
					v7.y += fontScale * (float)size2 * 0.05f;
					ref Vector2 v8 = ref glyphInfo.v1;
					v8.y += fontScale * (float)size2 * 0.05f;
				}
			}
			num13 = glyphInfo.v0.x + num2;
			num16 = glyphInfo.v0.y - num3;
			num14 = glyphInfo.v1.x + num2;
			num15 = glyphInfo.v1.y - num3;
			float num18 = glyphInfo.advance;
			if (finalSpacingX < 0f)
			{
				num18 += finalSpacingX;
			}
			if (Mathf.RoundToInt(num2 + num18) > regionWidth)
			{
				if (num2 == 0f)
				{
					return;
				}
				if (alignment != Alignment.Left && size < verts.size)
				{
					Align(verts, size, num2 - finalSpacingX);
					size = verts.size;
				}
				num13 -= num2;
				num14 -= num2;
				num16 -= maxLineHeight;
				num15 -= maxLineHeight;
				num2 = 0f;
				num3 += maxLineHeight;
				num11 = 0f;
				num10 = verts.size;
			}
			if (IsSpace(num))
			{
				if (underline)
				{
					num = 95;
				}
				else if (strike)
				{
					num = 45;
				}
			}
			num2 += ((sub != 0) ? ((finalSpacingX + glyphInfo.advance) * 0.75f) : (finalSpacingX + glyphInfo.advance));
			if (sub != 0)
			{
				num2 = Mathf.Round(num2);
			}
			if (IsSpace(num) && space == 0)
			{
				continue;
			}
			if (uvs != null)
			{
				if ((Object)(object)bitmapFont != (Object)null)
				{
					glyphInfo.u0.x = ((Rect)(ref val4)).xMin + num6 * glyphInfo.u0.x;
					glyphInfo.u2.x = ((Rect)(ref val4)).xMin + num6 * glyphInfo.u2.x;
					glyphInfo.u0.y = ((Rect)(ref val4)).yMax - num7 * glyphInfo.u0.y;
					glyphInfo.u2.y = ((Rect)(ref val4)).yMax - num7 * glyphInfo.u2.y;
					glyphInfo.u1.x = glyphInfo.u0.x;
					glyphInfo.u1.y = glyphInfo.u2.y;
					glyphInfo.u3.x = glyphInfo.u2.x;
					glyphInfo.u3.y = glyphInfo.u0.y;
				}
				int m = 0;
				for (int num19 = ((!flag2) ? 1 : 4); m < num19; m++)
				{
					uvs.Add(glyphInfo.u0);
					uvs.Add(glyphInfo.u1);
					uvs.Add(glyphInfo.u2);
					uvs.Add(glyphInfo.u3);
				}
			}
			if (cols != null)
			{
				if (glyphInfo.channel == 0 || glyphInfo.channel == 15)
				{
					if (gradient)
					{
						float num20 = num8 + glyphInfo.v0.y / fontScale;
						float num21 = num8 + glyphInfo.v1.y / fontScale;
						num20 /= num8;
						num21 /= num8;
						s_c0 = Color.Lerp(val, val2, num20);
						s_c1 = Color.Lerp(val, val2, num21);
						int n = 0;
						for (int num22 = ((!flag2) ? 1 : 4); n < num22; n++)
						{
							cols.Add(s_c0);
							cols.Add(s_c1);
							cols.Add(s_c1);
							cols.Add(s_c0);
						}
					}
					else
					{
						int num23 = 0;
						for (int num24 = ((!flag2) ? 4 : 16); num23 < num24; num23++)
						{
							cols.Add(item);
						}
					}
				}
				else
				{
					Color val6 = val3;
					val6 *= 0.49f;
					switch (glyphInfo.channel)
					{
					case 1:
						val6.b += 0.51f;
						break;
					case 2:
						val6.g += 0.51f;
						break;
					case 4:
						val6.r += 0.51f;
						break;
					case 8:
						val6.a += 0.51f;
						break;
					}
					Color item2 = val6.GammaToLinearSpace();
					int num25 = 0;
					for (int num26 = ((!flag2) ? 4 : 16); num25 < num26; num25++)
					{
						cols.Add(item2);
					}
				}
			}
			if (!flag2)
			{
				if (!italic)
				{
					verts.Add(new Vector3(num13, num16));
					verts.Add(new Vector3(num13, num15));
					verts.Add(new Vector3(num14, num15));
					verts.Add(new Vector3(num14, num16));
				}
				else
				{
					float num27 = (float)size2 * 0.1f * ((num15 - num16) / (float)size2);
					verts.Add(new Vector3(num13 - num27, num16));
					verts.Add(new Vector3(num13 + num27, num15));
					verts.Add(new Vector3(num14 + num27, num15));
					verts.Add(new Vector3(num14 - num27, num16));
				}
			}
			else
			{
				for (int num28 = 0; num28 < 4; num28++)
				{
					float num29 = mBoldOffset[num28 * 2];
					float num30 = mBoldOffset[num28 * 2 + 1];
					float num31 = ((!italic) ? 0f : ((float)size2 * 0.1f * ((num15 - num16) / (float)size2)));
					verts.Add(new Vector3(num13 + num29 - num31, num16 + num30));
					verts.Add(new Vector3(num13 + num29 + num31, num15 + num30));
					verts.Add(new Vector3(num14 + num29 + num31, num15 + num30));
					verts.Add(new Vector3(num14 + num29 - num31, num16 + num30));
				}
			}
			if (!underline && !strike)
			{
				continue;
			}
			GlyphInfo glyphInfo2 = GetGlyph((!strike) ? 95 : 45, prev, size2);
			if (glyphInfo2 == null)
			{
				continue;
			}
			if (uvs != null)
			{
				if ((Object)(object)bitmapFont != (Object)null)
				{
					glyphInfo2.u0.x = ((Rect)(ref val4)).xMin + num6 * glyphInfo2.u0.x;
					glyphInfo2.u2.x = ((Rect)(ref val4)).xMin + num6 * glyphInfo2.u2.x;
					glyphInfo2.u0.y = ((Rect)(ref val4)).yMax - num7 * glyphInfo2.u0.y;
					glyphInfo2.u2.y = ((Rect)(ref val4)).yMax - num7 * glyphInfo2.u2.y;
				}
				float num32 = (glyphInfo2.u0.x + glyphInfo2.u2.x) * 0.5f;
				int num33 = 0;
				for (int num34 = ((!flag2) ? 1 : 4); num33 < num34; num33++)
				{
					uvs.Add(new Vector2(num32, glyphInfo2.u0.y));
					uvs.Add(new Vector2(num32, glyphInfo2.u2.y));
					uvs.Add(new Vector2(num32, glyphInfo2.u2.y));
					uvs.Add(new Vector2(num32, glyphInfo2.u0.y));
				}
			}
			if (flag && strike)
			{
				num16 = (0f - num3 + glyphInfo2.v0.y) * 0.75f;
				num15 = (0f - num3 + glyphInfo2.v1.y) * 0.75f;
			}
			else
			{
				num16 = 0f - num3 + glyphInfo2.v0.y;
				num15 = 0f - num3 + glyphInfo2.v1.y;
			}
			if (flag2)
			{
				for (int num35 = 0; num35 < 4; num35++)
				{
					float num36 = mBoldOffset[num35 * 2];
					float num37 = mBoldOffset[num35 * 2 + 1];
					verts.Add(new Vector3(num11 + num36, num16 + num37));
					verts.Add(new Vector3(num11 + num36, num15 + num37));
					verts.Add(new Vector3(num2 + num36, num15 + num37));
					verts.Add(new Vector3(num2 + num36, num16 + num37));
				}
			}
			else
			{
				verts.Add(new Vector3(num11, num16));
				verts.Add(new Vector3(num11, num15));
				verts.Add(new Vector3(num2, num15));
				verts.Add(new Vector3(num2, num16));
			}
			if (gradient)
			{
				float num38 = num8 + glyphInfo2.v0.y / fontScale;
				float num39 = num8 + glyphInfo2.v1.y / fontScale;
				num38 /= num8;
				num39 /= num8;
				s_c0 = Color.Lerp(val, val2, num38);
				s_c1 = Color.Lerp(val, val2, num39);
				int num40 = 0;
				for (int num41 = ((!flag2) ? 1 : 4); num40 < num41; num40++)
				{
					cols.Add(s_c0);
					cols.Add(s_c1);
					cols.Add(s_c1);
					cols.Add(s_c0);
				}
			}
			else
			{
				int num42 = 0;
				for (int num43 = ((!flag2) ? 4 : 16); num42 < num43; num42++)
				{
					cols.Add(item);
				}
			}
		}
		ChangeBaseline(verts, start, verts.size, maxLineHeight.Get());
		if (alignment != Alignment.Left && size < verts.size)
		{
			Align(verts, size, num2 - finalSpacingX);
			size = verts.size;
		}
		mColors.Clear();
	}

	private static void ChangeBaseline(BetterList<Vector3> verts, int start, int end, float height)
	{
		//IL_002f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0034: Unknown result type (might be due to invalid IL or missing references)
		//IL_0046: Unknown result type (might be due to invalid IL or missing references)
		if (finalLineHeight != height)
		{
			float num = baseline * (finalLineHeight - height) / finalLineHeight * fontScale;
			for (int i = start; i < end; i++)
			{
				Vector3 value = verts[i];
				value.y += num;
				verts[i] = value;
			}
		}
	}

	public static void PrintApproximateCharacterPositions(string text, BetterList<Vector3> verts, BetterList<int> indices)
	{
		//IL_006b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0246: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		float num = 0f;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = (float)fontSize * fontScale * 0.5f;
		int length = text.Length;
		int size = verts.size;
		int num5 = 0;
		int prev = 0;
		for (int i = 0; i < length; i++)
		{
			num5 = text[i];
			verts.Add(new Vector3(num, 0f - num2 - num4));
			indices.Add(i);
			if (num5 == 10)
			{
				if (num > num3)
				{
					num3 = num;
				}
				if (alignment != Alignment.Left)
				{
					Align(verts, size, num - finalSpacingX, 1);
					size = verts.size;
				}
				num = 0f;
				num2 += finalLineHeight;
				prev = 0;
				continue;
			}
			if (num5 < 32)
			{
				prev = 0;
				continue;
			}
			if (encoding && ParseSymbol(text, ref i))
			{
				i--;
				continue;
			}
			BMSymbol bMSymbol = ((!useSymbols) ? null : GetSymbol(text, i, length));
			if (bMSymbol == null)
			{
				float glyphWidth = GetGlyphWidth(num5, prev);
				if (glyphWidth == 0f)
				{
					continue;
				}
				glyphWidth += finalSpacingX;
				if (Mathf.RoundToInt(num + glyphWidth) > regionWidth)
				{
					if (num == 0f)
					{
						return;
					}
					if (alignment != Alignment.Left && size < verts.size)
					{
						Align(verts, size, num - finalSpacingX, 1);
						size = verts.size;
					}
					num = glyphWidth;
					num2 += finalLineHeight;
				}
				else
				{
					num += glyphWidth;
				}
				verts.Add(new Vector3(num, 0f - num2 - num4));
				indices.Add(i + 1);
				prev = num5;
				continue;
			}
			float num6 = (float)bMSymbol.advance * fontScale + finalSpacingX;
			if (Mathf.RoundToInt(num + num6) > regionWidth)
			{
				if (num == 0f)
				{
					return;
				}
				if (alignment != Alignment.Left && size < verts.size)
				{
					Align(verts, size, num - finalSpacingX, 1);
					size = verts.size;
				}
				num = num6;
				num2 += finalLineHeight;
			}
			else
			{
				num += num6;
			}
			verts.Add(new Vector3(num, 0f - num2 - num4));
			indices.Add(i + 1);
			i += bMSymbol.sequence.Length - 1;
			prev = 0;
		}
		if (alignment != Alignment.Left && size < verts.size)
		{
			Align(verts, size, num - finalSpacingX, 1);
		}
	}

	public static void PrintExactCharacterPositions(string text, BetterList<Vector3> verts, BetterList<int> indices)
	{
		//IL_0252: Unknown result type (might be due to invalid IL or missing references)
		//IL_0263: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01ab: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		float num = (float)fontSize * fontScale;
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		int length = text.Length;
		int size = verts.size;
		int num5 = 0;
		int prev = 0;
		for (int i = 0; i < length; i++)
		{
			num5 = text[i];
			if (num5 == 10)
			{
				if (num2 > num4)
				{
					num4 = num2;
				}
				if (alignment != Alignment.Left)
				{
					Align(verts, size, num2 - finalSpacingX, 2);
					size = verts.size;
				}
				num2 = 0f;
				num3 += finalLineHeight;
				prev = 0;
				continue;
			}
			if (num5 < 32)
			{
				prev = 0;
				continue;
			}
			if (encoding && ParseSymbol(text, ref i))
			{
				i--;
				continue;
			}
			BMSymbol bMSymbol = ((!useSymbols) ? null : GetSymbol(text, i, length));
			if (bMSymbol == null)
			{
				float glyphWidth = GetGlyphWidth(num5, prev);
				if (glyphWidth == 0f)
				{
					continue;
				}
				float num6 = glyphWidth + finalSpacingX;
				if (Mathf.RoundToInt(num2 + num6) > regionWidth)
				{
					if (num2 == 0f)
					{
						return;
					}
					if (alignment != Alignment.Left && size < verts.size)
					{
						Align(verts, size, num2 - finalSpacingX, 2);
						size = verts.size;
					}
					num2 = 0f;
					num3 += finalLineHeight;
					prev = 0;
					i--;
				}
				else
				{
					indices.Add(i);
					verts.Add(new Vector3(num2, 0f - num3 - num));
					verts.Add(new Vector3(num2 + num6, 0f - num3));
					prev = num5;
					num2 += num6;
				}
				continue;
			}
			float num7 = (float)bMSymbol.advance * fontScale + finalSpacingX;
			if (Mathf.RoundToInt(num2 + num7) > regionWidth)
			{
				if (num2 == 0f)
				{
					return;
				}
				if (alignment != Alignment.Left && size < verts.size)
				{
					Align(verts, size, num2 - finalSpacingX, 2);
					size = verts.size;
				}
				num2 = 0f;
				num3 += finalLineHeight;
				prev = 0;
				i--;
			}
			else
			{
				indices.Add(i);
				verts.Add(new Vector3(num2, 0f - num3 - num));
				verts.Add(new Vector3(num2 + num7, 0f - num3));
				i += bMSymbol.sequence.Length - 1;
				num2 += num7;
				prev = 0;
			}
		}
		if (alignment != Alignment.Left && size < verts.size)
		{
			Align(verts, size, num2 - finalSpacingX, 2);
		}
	}

	public static void PrintCaretAndSelection(string text, int start, int end, BetterList<Vector3> caret, BetterList<Vector3> highlight)
	{
		//IL_0088: Unknown result type (might be due to invalid IL or missing references)
		//IL_008d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0094: Unknown result type (might be due to invalid IL or missing references)
		//IL_04bf: Unknown result type (might be due to invalid IL or missing references)
		//IL_04d3: Unknown result type (might be due to invalid IL or missing references)
		//IL_04e7: Unknown result type (might be due to invalid IL or missing references)
		//IL_04fe: Unknown result type (might be due to invalid IL or missing references)
		//IL_00c0: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00e8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ff: Unknown result type (might be due to invalid IL or missing references)
		//IL_0533: Unknown result type (might be due to invalid IL or missing references)
		//IL_0535: Unknown result type (might be due to invalid IL or missing references)
		//IL_0541: Unknown result type (might be due to invalid IL or missing references)
		//IL_0543: Unknown result type (might be due to invalid IL or missing references)
		//IL_056a: Unknown result type (might be due to invalid IL or missing references)
		//IL_0579: Unknown result type (might be due to invalid IL or missing references)
		//IL_058e: Unknown result type (might be due to invalid IL or missing references)
		//IL_05a6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0163: Unknown result type (might be due to invalid IL or missing references)
		//IL_0165: Unknown result type (might be due to invalid IL or missing references)
		//IL_0171: Unknown result type (might be due to invalid IL or missing references)
		//IL_0173: Unknown result type (might be due to invalid IL or missing references)
		//IL_019a: Unknown result type (might be due to invalid IL or missing references)
		//IL_01a9: Unknown result type (might be due to invalid IL or missing references)
		//IL_01be: Unknown result type (might be due to invalid IL or missing references)
		//IL_01d6: Unknown result type (might be due to invalid IL or missing references)
		//IL_0433: Unknown result type (might be due to invalid IL or missing references)
		//IL_0435: Unknown result type (might be due to invalid IL or missing references)
		//IL_0441: Unknown result type (might be due to invalid IL or missing references)
		//IL_0443: Unknown result type (might be due to invalid IL or missing references)
		//IL_0462: Unknown result type (might be due to invalid IL or missing references)
		//IL_0472: Unknown result type (might be due to invalid IL or missing references)
		//IL_0327: Unknown result type (might be due to invalid IL or missing references)
		//IL_0329: Unknown result type (might be due to invalid IL or missing references)
		//IL_0335: Unknown result type (might be due to invalid IL or missing references)
		//IL_0337: Unknown result type (might be due to invalid IL or missing references)
		//IL_035e: Unknown result type (might be due to invalid IL or missing references)
		//IL_036d: Unknown result type (might be due to invalid IL or missing references)
		//IL_0382: Unknown result type (might be due to invalid IL or missing references)
		//IL_039a: Unknown result type (might be due to invalid IL or missing references)
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		int num = end;
		if (start > end)
		{
			end = start;
			start = num;
		}
		float num2 = 0f;
		float num3 = 0f;
		float num4 = 0f;
		float num5 = (float)fontSize * fontScale;
		int indexOffset = caret?.size ?? 0;
		int num6 = highlight?.size ?? 0;
		int length = text.Length;
		int i = 0;
		int num7 = 0;
		int prev = 0;
		bool flag = false;
		bool flag2 = false;
		Vector2 zero = Vector2.zero;
		Vector2 zero2 = Vector2.zero;
		for (; i < length; i++)
		{
			if (caret != null && !flag2 && num <= i)
			{
				flag2 = true;
				caret.Add(new Vector3(num2 - 1f, 0f - num3 - num5));
				caret.Add(new Vector3(num2 - 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3 - num5));
			}
			num7 = text[i];
			if (num7 == 10)
			{
				if (num2 > num4)
				{
					num4 = num2;
				}
				if (caret != null && flag2)
				{
					if (alignment != Alignment.Left)
					{
						Align(caret, indexOffset, num2 - finalSpacingX);
					}
					caret = null;
				}
				if (highlight != null)
				{
					if (flag)
					{
						flag = false;
						highlight.Add(Vector2.op_Implicit(zero2));
						highlight.Add(Vector2.op_Implicit(zero));
					}
					else if (start <= i && end > i)
					{
						highlight.Add(new Vector3(num2, 0f - num3 - num5));
						highlight.Add(new Vector3(num2, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3 - num5));
					}
					if (alignment != Alignment.Left && num6 < highlight.size)
					{
						Align(highlight, num6, num2 - finalSpacingX);
						num6 = highlight.size;
					}
				}
				num2 = 0f;
				num3 += finalLineHeight;
				prev = 0;
				continue;
			}
			if (num7 < 32)
			{
				prev = 0;
				continue;
			}
			if (encoding && ParseSymbol(text, ref i))
			{
				i--;
				continue;
			}
			BMSymbol bMSymbol = ((!useSymbols) ? null : GetSymbol(text, i, length));
			float num8 = ((bMSymbol == null) ? GetGlyphWidth(num7, prev) : ((float)bMSymbol.advance * fontScale));
			if (num8 == 0f)
			{
				continue;
			}
			float num9 = num2;
			float num10 = num2 + num8;
			float num11 = 0f - num3 - num5;
			float num12 = 0f - num3;
			if (Mathf.RoundToInt(num10 + finalSpacingX) > regionWidth)
			{
				if (num2 == 0f)
				{
					return;
				}
				if (num2 > num4)
				{
					num4 = num2;
				}
				if (caret != null && flag2)
				{
					if (alignment != Alignment.Left)
					{
						Align(caret, indexOffset, num2 - finalSpacingX);
					}
					caret = null;
				}
				if (highlight != null)
				{
					if (flag)
					{
						flag = false;
						highlight.Add(Vector2.op_Implicit(zero2));
						highlight.Add(Vector2.op_Implicit(zero));
					}
					else if (start <= i && end > i)
					{
						highlight.Add(new Vector3(num2, 0f - num3 - num5));
						highlight.Add(new Vector3(num2, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3 - num5));
					}
					if (alignment != Alignment.Left && num6 < highlight.size)
					{
						Align(highlight, num6, num2 - finalSpacingX);
						num6 = highlight.size;
					}
				}
				num9 -= num2;
				num10 -= num2;
				num11 -= finalLineHeight;
				num12 -= finalLineHeight;
				num2 = 0f;
				num3 += finalLineHeight;
			}
			num2 += num8 + finalSpacingX;
			if (highlight != null)
			{
				if (start > i || end <= i)
				{
					if (flag)
					{
						flag = false;
						highlight.Add(Vector2.op_Implicit(zero2));
						highlight.Add(Vector2.op_Implicit(zero));
					}
				}
				else if (!flag)
				{
					flag = true;
					highlight.Add(new Vector3(num9, num11));
					highlight.Add(new Vector3(num9, num12));
				}
			}
			((Vector2)(ref zero))._002Ector(num10, num11);
			((Vector2)(ref zero2))._002Ector(num10, num12);
			prev = num7;
		}
		if (caret != null)
		{
			if (!flag2)
			{
				caret.Add(new Vector3(num2 - 1f, 0f - num3 - num5));
				caret.Add(new Vector3(num2 - 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3));
				caret.Add(new Vector3(num2 + 1f, 0f - num3 - num5));
			}
			if (alignment != Alignment.Left)
			{
				Align(caret, indexOffset, num2 - finalSpacingX);
			}
		}
		if (highlight != null)
		{
			if (flag)
			{
				highlight.Add(Vector2.op_Implicit(zero2));
				highlight.Add(Vector2.op_Implicit(zero));
			}
			else if (start < i && end == i)
			{
				highlight.Add(new Vector3(num2, 0f - num3 - num5));
				highlight.Add(new Vector3(num2, 0f - num3));
				highlight.Add(new Vector3(num2 + 2f, 0f - num3));
				highlight.Add(new Vector3(num2 + 2f, 0f - num3 - num5));
			}
			if (alignment != Alignment.Left && num6 < highlight.size)
			{
				Align(highlight, num6, num2 - finalSpacingX);
			}
		}
	}

	public static bool ReplaceLink(ref string text, ref int index, string prefix)
	{
		if (index == -1)
		{
			return false;
		}
		index = text.IndexOf(prefix, index);
		if (index == -1)
		{
			return false;
		}
		int num = index + prefix.Length;
		int num2 = text.IndexOf(' ', num);
		if (num2 == -1)
		{
			num2 = text.Length;
		}
		int num3 = text.IndexOfAny(new char[2] { '/', ' ' }, num);
		if (num3 == -1 || num3 == num)
		{
			index += 7;
			return true;
		}
		string text2 = text.Substring(0, index);
		string text3 = text.Substring(index, num2 - index);
		string text4 = text.Substring(num2);
		string text5 = text.Substring(num, num3 - num);
		text = text2 + "[url=" + text3 + "][u]" + text5 + "[/u][/url]";
		index = text.Length;
		text += text4;
		return true;
	}

	public static bool InsertHyperlink(ref string text, ref int index, string keyword, string link)
	{
		int num = text.IndexOf(keyword, index, StringComparison.CurrentCultureIgnoreCase);
		if (num == -1)
		{
			return false;
		}
		string text2 = text.Substring(0, num);
		string text3 = "[url=" + link + "][u]";
		string text4 = text.Substring(num, keyword.Length) + "[/u][/url]";
		string text5 = text.Substring(num + keyword.Length);
		text = text2 + text3 + text4;
		index = text.Length;
		text += text5;
		return true;
	}

	public static void ReplaceLinks(ref string text)
	{
		int index = 0;
		while (index < text.Length && ReplaceLink(ref text, ref index, "http://"))
		{
		}
		int index2 = 0;
		while (index2 < text.Length && ReplaceLink(ref text, ref index2, "https://"))
		{
		}
	}
}
