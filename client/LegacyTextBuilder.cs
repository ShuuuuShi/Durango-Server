using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using UnityEngine;

public static class LegacyTextBuilder
{
	private struct MaxLineHeight
	{
		private float _value;

		private float _last;

		public MaxLineHeight(float val)
		{
			_value = 0f;
			_last = val;
		}

		public void Reset()
		{
			_value = 0f;
		}

		public void Set(float val)
		{
			_value = Mathf.Max(_value, val);
			_last = val;
		}

		public float Get()
		{
			return (!(_value > 0f)) ? _last : _value;
		}

		public static float operator +(float v1, MaxLineHeight v2)
		{
			return v1 + v2.Get();
		}

		public static float operator -(float v1, MaxLineHeight v2)
		{
			return v1 - v2.Get();
		}
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

	private static FontStyle _fontStyle = FontStyle.Normal;

	public static NGUIText.Alignment alignment = NGUIText.Alignment.Left;

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

	private static Stack<int> fontSizeStack = new Stack<int>();

	private static Stack<int> symbolStack = new Stack<int>();

	private static StringBuilder stringBuilder = new StringBuilder();

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
			return FontStyle.Normal;
		}
		set
		{
			_fontStyle = value;
		}
	}

	public static void Update()
	{
		Update(request: true);
	}

	public static void Update(bool request)
	{
		finalSize = Mathf.RoundToInt((float)fontSize / pixelDensity);
		finalSpacingX = spacingX * fontScale;
		finalLineHeight = ((float)fontSize + spacingY) * fontScale;
		useSymbols = bitmapFont != null && bitmapFont.hasSymbols && encoding && symbolStyle != SymbolStyle.None;
		Font font = dynamicFont;
		if (!(font != null) || !request)
		{
			return;
		}
		font.RequestCharactersInTexture(")_-.", finalSize, fontStyle);
		if (!font.GetCharacterInfo(')', out mTempChar, finalSize, fontStyle) || (float)mTempChar.maxY == 0f)
		{
			font.RequestCharactersInTexture("A", finalSize, fontStyle);
			if (!font.GetCharacterInfo('A', out mTempChar, finalSize, fontStyle))
			{
				baseline = 0f;
				return;
			}
		}
		float num = mTempChar.maxY;
		float num2 = mTempChar.minY;
		baseline = Mathf.Round(num + ((float)finalSize - num + num2) * 0.5f);
	}

	public static void Prepare(string text)
	{
		if (!(dynamicFont != null))
		{
			return;
		}
		if (encoding)
		{
			fontSizeStack.Clear();
			int length = text.Length;
			int size = finalSize;
			int size2 = size;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < length; i++)
			{
				num2 = i;
				int space;
				if (ParseSize(text, ref i, ref size))
				{
					if (num < num2)
					{
						dynamicFont.RequestCharactersInTexture(text.Substring(num, num2 - num), size2, fontStyle);
					}
					size2 = size;
					num = i;
					i--;
				}
				else if (ParseSpace(text, size, ref i, out space))
				{
					dynamicFont.RequestCharactersInTexture(" ", size2, fontStyle);
					i--;
				}
				else if (ParseSymbol(text, ref i))
				{
					i--;
				}
			}
			num2++;
			if (num < num2)
			{
				dynamicFont.RequestCharactersInTexture(text.Substring(num, num2 - num), size, fontStyle);
			}
		}
		else
		{
			dynamicFont.RequestCharactersInTexture(text, finalSize, fontStyle);
		}
	}

	public static BMSymbol GetSymbol(string text, int index, int textLength)
	{
		return (!(bitmapFont != null)) ? null : bitmapFont.MatchSymbol(text, index, textLength);
	}

	public static float GetGlyphWidth(int ch, int prev)
	{
		return GetGlyphWidth(ch, prev, finalSize);
	}

	public static float GetGlyphWidth(int ch, int prev, int fontSize)
	{
		if (bitmapFont != null)
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
		else if (dynamicFont != null && dynamicFont.GetCharacterInfo((char)ch, out mTempChar, fontSize, fontStyle))
		{
			return (float)mTempChar.advance * fontScale * pixelDensity;
		}
		return 0f;
	}

	public static GlyphInfo GetGlyph(int ch, int prev)
	{
		return GetGlyph(ch, prev, finalSize);
	}

	public static GlyphInfo GetGlyph(int ch, int prev, int size)
	{
		if (bitmapFont != null)
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
					glyph.v0 *= fontScale;
					glyph.v1 *= fontScale;
					glyph.advance *= fontScale;
				}
				return glyph;
			}
		}
		else if (dynamicFont != null && dynamicFont.GetCharacterInfo((char)ch, out mTempChar, size, fontStyle))
		{
			glyph.v0.x = mTempChar.minX;
			glyph.v1.x = mTempChar.maxX;
			glyph.v0.y = (float)mTempChar.maxY - baseline;
			glyph.v1.y = (float)mTempChar.minY - baseline;
			glyph.u0 = mTempChar.uvTopLeft;
			glyph.u1 = mTempChar.uvBottomLeft;
			glyph.u2 = mTempChar.uvBottomRight;
			glyph.u3 = mTempChar.uvTopRight;
			glyph.advance = mTempChar.advance;
			glyph.channel = 0;
			glyph.v0.x = Mathf.Round(glyph.v0.x);
			glyph.v0.y = Mathf.Round(glyph.v0.y);
			glyph.v1.x = Mathf.Round(glyph.v1.x);
			glyph.v1.y = Mathf.Round(glyph.v1.y);
			float num3 = fontScale * pixelDensity;
			if (num3 != 1f)
			{
				glyph.v0 *= num3;
				glyph.v1 *= num3;
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

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static Color ParseColor(string text, int offset = 0)
	{
		return ParseColor24(text, offset);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static Color ParseColor24(string text, int offset = 0)
	{
		int num = (NGUIMath.HexToDecimal(text[offset]) << 4) | NGUIMath.HexToDecimal(text[offset + 1]);
		int num2 = (NGUIMath.HexToDecimal(text[offset + 2]) << 4) | NGUIMath.HexToDecimal(text[offset + 3]);
		int num3 = (NGUIMath.HexToDecimal(text[offset + 4]) << 4) | NGUIMath.HexToDecimal(text[offset + 5]);
		float num4 = 0.003921569f;
		return new Color(num4 * (float)num, num4 * (float)num2, num4 * (float)num3);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static Color ParseColor32(string text, int offset)
	{
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
		return EncodeColor24(c);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static string EncodeColor(string text, Color c)
	{
		return "[c][" + EncodeColor24(c) + "]" + text + "[-][/c]";
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static string EncodeAlpha(float a)
	{
		int num = Mathf.Clamp(Mathf.RoundToInt(a * 255f), 0, 255);
		return NGUIMath.DecimalToHex8(num);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static string EncodeColor24(Color c)
	{
		int num = 0xFFFFFF & (NGUIMath.ColorToInt(c) >> 8);
		return NGUIMath.DecimalToHex24(num);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static string EncodeColor32(Color c)
	{
		int num = NGUIMath.ColorToInt(c);
		return NGUIMath.DecimalToHex32(num);
	}

	public static bool ParseSymbol(string text, ref int index)
	{
		int sub = 1;
		int bold = 0;
		int italic = 0;
		int underline = 0;
		int strike = 0;
		int ignoreColor = 0;
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
					switch (text[index + 6])
					{
					case '+':
					{
						if (int.TryParse(text.Substring(index + 7, num - (index + 7)), out var result4))
						{
							fontSizeStack.Push(size);
							size += result4;
							index = num + 1;
							return true;
						}
						break;
					}
					case '-':
					{
						if (int.TryParse(text.Substring(index + 7, num - (index + 7)), out var result5))
						{
							fontSizeStack.Push(size);
							size -= result5;
							index = num + 1;
							return true;
						}
						break;
					}
					case '*':
					{
						if (float.TryParse(text.Substring(index + 7, num - (index + 7)), out var result2))
						{
							fontSizeStack.Push(size);
							size = Mathf.RoundToInt((float)size * result2);
							index = num + 1;
							return true;
						}
						break;
					}
					case '/':
					{
						if (float.TryParse(text.Substring(index + 7, num - (index + 7)), out var result3) && result3 > 0f)
						{
							fontSizeStack.Push(size);
							size = Mathf.RoundToInt((float)size / result3);
							index = num + 1;
							return true;
						}
						break;
					}
					default:
					{
						if (int.TryParse(text.Substring(index + 6, num - (index + 6)), out var result))
						{
							fontSizeStack.Push(size);
							size = result;
							index = num + 1;
							return true;
						}
						break;
					}
					}
				}
			}
		}
		return false;
	}

	public static bool ParseSymbol(string text, ref int index, BetterList<Color> colors, bool premultiply, ref int sub, ref int bold, ref int italic, ref int underline, ref int strike, ref int ignoreColor)
	{
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
				bold++;
				index += 3;
				return true;
			case "[i]":
				italic++;
				index += 3;
				return true;
			case "[u]":
				underline++;
				index += 3;
				return true;
			case "[s]":
				strike++;
				index += 3;
				return true;
			case "[c]":
				ignoreColor++;
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
				bold = Mathf.Max(bold - 1, 0);
				index += 4;
				return true;
			case "[/i]":
				italic = Mathf.Max(italic - 1, 0);
				index += 4;
				return true;
			case "[/u]":
				underline = Mathf.Max(underline - 1, 0);
				index += 4;
				return true;
			case "[/s]":
				strike = Mathf.Max(strike - 1, 0);
				index += 4;
				return true;
			case "[/c]":
				ignoreColor = Mathf.Max(ignoreColor - 1, 0);
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
		if (text[index + 1] == 'c' && text[index + 2] == '=')
		{
			int num2 = text.IndexOf(']', index + 2);
			if (num2 != -1)
			{
				int num3 = index + 3;
				if (colors != null)
				{
					string key = text.Substring(num3, num2 - num3);
					if (PresetColor.TryGet(key, out var color))
					{
						color.a = colors[colors.size - 1].a;
						if (premultiply && color.a != 1f)
						{
							color = Color.Lerp(mInvisible, color, color.a);
						}
						colors.Add(color);
					}
				}
				index = num2 + 1;
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
			int num4 = text.IndexOf(']', index + 4);
			if (num4 != -1)
			{
				index = num4 + 1;
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
			Color color2 = ParseColor24(text, index + 1);
			if (!IsColorEncoded(text, index + 1, 6))
			{
				return false;
			}
			if (colors != null)
			{
				color2.a = colors[colors.size - 1].a;
				if (premultiply && color2.a != 1f)
				{
					color2 = Color.Lerp(mInvisible, color2, color2.a);
				}
				colors.Add(color2);
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
			Color color3 = ParseColor32(text, index + 1);
			if (!IsColorEncoded(text, index + 1, 8))
			{
				return false;
			}
			if (colors != null)
			{
				if (premultiply && color3.a != 1f)
				{
					color3 = Color.Lerp(mInvisible, color3, color3.a);
				}
				colors.Add(color3);
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
			symbolStack.Clear();
			int num = 0;
			int length = text.Length;
			while (num < length)
			{
				switch (text[num])
				{
				case '[':
				{
					int sub = 0;
					int bold = 0;
					int italic = 0;
					int underline = 0;
					int strike = 0;
					int ignoreColor = 0;
					int index = num;
					if (ParseSymbol(text, ref index, null, premultiply: false, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor))
					{
						text = text.Remove(num, index - num);
						length = text.Length;
						if (symbolStack.Count > 0)
						{
							num = symbolStack.Pop();
						}
						continue;
					}
					int size = 0;
					if (ParseSize(text, ref index, ref size))
					{
						text = text.Remove(num, index - num);
						length = text.Length;
						if (symbolStack.Count > 0)
						{
							num = symbolStack.Pop();
						}
						continue;
					}
					symbolStack.Push(num);
					break;
				}
				case ']':
					if (symbolStack.Count > 0)
					{
						symbolStack.Pop();
					}
					break;
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
		case NGUIText.Alignment.Right:
		{
			float num13 = (float)rectWidth - printedWidth;
			if (!(num13 < 0f))
			{
				for (int j = indexOffset; j < verts.size; j++)
				{
					verts.buffer[j].x += num13;
				}
			}
			break;
		}
		case NGUIText.Alignment.Center:
		{
			float num10 = ((float)rectWidth - printedWidth) * 0.5f;
			if (!(num10 < 0f))
			{
				int num11 = Mathf.RoundToInt((float)rectWidth - printedWidth);
				int num12 = Mathf.RoundToInt(rectWidth);
				bool flag = (num11 & 1) == 1;
				bool flag2 = (num12 & 1) == 1;
				if ((flag && !flag2) || (!flag && flag2))
				{
					num10 += 0.5f * fontScale;
				}
				for (int i = indexOffset; i < verts.size; i++)
				{
					verts.buffer[i].x += num10;
				}
			}
			break;
		}
		case NGUIText.Alignment.Justified:
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
				float a = num8 + num7;
				float num9 = x2 * num4;
				float b = num9 - num7;
				float t = (float)num6 * num3;
				x2 = Mathf.Lerp(a, num9, t);
				x = Mathf.Lerp(num8, b, t);
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

	[DebuggerHidden]
	[DebuggerStepThrough]
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
			zero.y = num2 + maxLineHeight - spacingY * fontScale;
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

	public static bool WrapText(string text, out string finalText, bool keepCharCount, bool wrapLineColors, bool useEllipsis = false, bool newLinePriority = false)
	{
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
		float num3 = spacingY * fontScale;
		int num4 = 0;
		int i = 0;
		int num5 = 1;
		int prev = 0;
		bool flag = false;
		int length = text.Length;
		int num6 = ((maxLines <= 0) ? 1000000 : maxLines);
		if (string.IsNullOrEmpty(text))
		{
			text = " ";
		}
		Prepare(text);
		if (encoding)
		{
			num5 = 0;
			for (; i < length; i++)
			{
				char c = text[i];
				bool flag2 = c > '\u2fff';
				if (c == '\n')
				{
					num = regionWidth;
					flag = false;
					if (num5 + 1 == num6)
					{
						break;
					}
					num2 -= maxLineHeight.Get();
					if (num2 < 0f - num3)
					{
						break;
					}
					num5++;
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
				float num7;
				if (encoding && ParseSpace(text, size, ref i, out space))
				{
					num7 = space;
				}
				else if (bMSymbol == null)
				{
					float glyphWidth = GetGlyphWidth(c, prev, size);
					if (glyphWidth == 0f && !IsSpace(c))
					{
						continue;
					}
					num7 = finalSpacingX + glyphWidth;
				}
				else
				{
					num7 = finalSpacingX + (float)bMSymbol.advance * fontScale;
				}
				num -= num7;
				if (IsSpace(c) && !flag2 && num4 < i)
				{
					num4 = i + 1;
					flag = true;
				}
				if (Mathf.RoundToInt(num) < 0)
				{
					num = regionWidth;
					i = ((!flag) ? i : (num4 - 1));
					flag = false;
					prev = 0;
					if (num5 + 1 == num6)
					{
						break;
					}
					num2 -= maxLineHeight;
					if (num2 < 0f - num3)
					{
						break;
					}
					num5++;
					maxLineHeight.Reset();
				}
				else if (bMSymbol != null)
				{
					i += bMSymbol.length - 1;
				}
			}
			if (num2 - maxLineHeight >= 0f - num3)
			{
				num5++;
			}
			num6 = num5;
		}
		else
		{
			float num8 = ((maxLines <= 0) ? ((float)regionHeight) : Mathf.Min(regionHeight, finalLineHeight * (float)maxLines));
			num6 = Mathf.FloorToInt(Mathf.Min(num6, (num8 + num3) / finalLineHeight) + 0.01f);
		}
		if (num6 == 0)
		{
			finalText = string.Empty;
			return false;
		}
		StringBuilder s = stringBuilder;
		s.Length = 0;
		symbolStack.Clear();
		bool flag3 = true;
		bool flag4 = true;
		Color color = tint;
		int sub = 0;
		int bold = 0;
		int italic = 0;
		int underline = 0;
		int strike = 0;
		int ignoreColor = 0;
		if (!useSymbols)
		{
			wrapLineColors = false;
		}
		if (wrapLineColors)
		{
			mColors.Add(color);
			s.Append("[");
			s.Append(EncodeColor(color));
			s.Append("]");
		}
		num4 = 0;
		i = 0;
		num5 = 1;
		prev = 0;
		num = regionWidth;
		for (; i < length; i++)
		{
			char c2 = text[i];
			bool flag5 = c2 > '\u2fff';
			if (c2 == '\n')
			{
				if (num5 == num6)
				{
					break;
				}
				num = regionWidth;
				if (num4 < i)
				{
					s.Append(text.Substring(num4, i - num4 + 1));
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
				num5++;
				num4 = i + 1;
				prev = 0;
				continue;
			}
			if (encoding)
			{
				int t = i;
				bool flag6 = false;
				if (!wrapLineColors && ParseSymbol(text, ref i))
				{
					flag6 = true;
				}
				else if (ParseSymbol(text, ref i, mColors, premultiply, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor))
				{
					if (ignoreColor > 0)
					{
						color = mColors[mColors.size - 1];
						color.a *= mAlpha * tint.a;
					}
					else
					{
						color = tint * mColors[mColors.size - 1];
						color.a *= mAlpha;
					}
					int l = 0;
					for (int num9 = mColors.size - 2; l < num9; l++)
					{
						color.a *= mColors[l].a;
					}
					flag6 = true;
				}
				else if (ParseSize(text, ref i, ref size))
				{
					flag6 = true;
				}
				if (flag6)
				{
					i--;
					symbolStack.Push(t);
					symbolStack.Push(i);
					continue;
				}
			}
			BMSymbol bMSymbol2 = ((!useSymbols) ? null : GetSymbol(text, i, length));
			int space2 = 0;
			int num10 = i;
			float num11;
			if (encoding && ParseSpace(text, size, ref i, out space2))
			{
				num11 = space2;
			}
			else if (bMSymbol2 == null)
			{
				float glyphWidth2 = GetGlyphWidth(c2, prev, size);
				if (glyphWidth2 == 0f && !IsSpace(c2))
				{
					continue;
				}
				num11 = finalSpacingX + glyphWidth2;
			}
			else
			{
				num11 = finalSpacingX + (float)bMSymbol2.advance * fontScale;
			}
			num -= num11;
			if (IsSpace(c2) && !flag5 && num4 < i)
			{
				int num12 = i - num4 + 1;
				if (num5 == num6 && num <= 0f && i < length)
				{
					char c3 = text[i];
					if (c3 < ' ' || IsSpace(c3))
					{
						num12--;
					}
				}
				s.Append(text.Substring(num4, num12));
				flag3 = false;
				num4 = i + 1;
				prev = c2;
			}
			if (Mathf.RoundToInt(num) < 0)
			{
				if (!flag3 && num5 != num6)
				{
					flag3 = true;
					num = regionWidth;
					i = num4 - 1;
					prev = 0;
					if (num5++ == num6)
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
				if (useEllipsis && num5 == num6 && i > 1)
				{
					float num13 = GetGlyphWidth(46, 46) * 3f;
					if (num13 < (float)regionWidth)
					{
						num += num11;
						int num14 = ((space2 <= 0) ? i : num10);
						int num15 = 0;
						int num16 = ((symbolStack.Count <= 0) ? (-1) : symbolStack.Pop());
						bool flag7 = false;
						while (num14 > 1)
						{
							num14--;
							if (flag7)
							{
								if (num14 < num16)
								{
									num16 = ((symbolStack.Count <= 0) ? (-1) : symbolStack.Pop());
									flag7 = false;
									num14++;
								}
								else if (num14 < num4)
								{
									num15++;
								}
								continue;
							}
							if (num14 <= num16)
							{
								num16 = ((symbolStack.Count <= 0) ? (-1) : symbolStack.Pop());
								flag7 = true;
								num14++;
								continue;
							}
							if (num >= num13)
							{
								num14++;
								break;
							}
							char prev2 = text[num14 - 1];
							char ch = text[num14];
							bool flag8 = num == 0f && IsSpace(ch);
							num += GetGlyphWidth(ch, prev2);
							if (num14 < num4 && !flag8)
							{
								num15++;
							}
						}
						if (num >= num13)
						{
							if (num15 > 0)
							{
								s.Length = Mathf.Max(0, s.Length - num15);
							}
							s.Append(text.Substring(num4, Mathf.Max(0, num14 - num4)));
							while (s.Length > 0 && IsSpace(s[s.Length - 1]))
							{
								s.Length--;
							}
							s.Append("...");
							num5++;
							num4 = (i = num14);
							break;
						}
					}
				}
				s.Append(text.Substring(num4, Mathf.Max(0, ((space2 <= 0) ? i : num10) - num4)));
				bool flag9 = IsSpace(c2);
				if (!newLinePriority && !flag9 && !flag5)
				{
					flag4 = false;
				}
				if (wrapLineColors && mColors.size > 0)
				{
					s.Append("[-]");
				}
				if (num5++ == num6)
				{
					num4 = i;
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
					for (int num17 = 0; num17 < mColors.size; num17++)
					{
						s.Insert(s.Length - 1, "[-]");
					}
					for (int num18 = 0; num18 < mColors.size; num18++)
					{
						s.Append("[");
						s.Append(EncodeColor(mColors[num18]));
						s.Append("]");
					}
				}
				flag3 = true;
				if (flag9 || space2 > 0)
				{
					num4 = i + 1;
					num = regionWidth;
				}
				else
				{
					num4 = i;
					num = (float)regionWidth - num11;
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
		if (num4 < i)
		{
			s.Append(text.Substring(num4, i - num4));
		}
		if (wrapLineColors && mColors.size > 0)
		{
			s.Append("[-]");
		}
		finalText = s.ToString();
		mColors.Clear();
		return flag4 && (i == length || num5 <= Mathf.Min(maxLines, num6));
	}

	public static void Print(string text, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
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
		Color a = (tint * gradientBottom).GammaToLinearSpace();
		Color b = (tint * gradientTop).GammaToLinearSpace();
		Color color = tint;
		Color item = color.GammaToLinearSpace();
		int length = text.Length;
		Rect rect = default(Rect);
		float num6 = 0f;
		float num7 = 0f;
		float num8 = num5 * pixelDensity;
		bool flag = false;
		int sub = 0;
		int bold = 0;
		int italic = 0;
		int underline = 0;
		int strike = 0;
		int ignoreColor = 0;
		int size2 = finalSize;
		fontSizeStack.Clear();
		MaxLineHeight maxLineHeight = new MaxLineHeight(finalLineHeight);
		float val = maxLineHeight.Get();
		float num9 = 0f;
		int start = size;
		int num10 = 0;
		float num11 = 0f;
		if (bitmapFont != null)
		{
			rect = bitmapFont.uvRect;
			num6 = rect.width / (float)bitmapFont.texWidth;
			num7 = rect.height / (float)bitmapFont.texHeight;
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
				if (alignment != NGUIText.Alignment.Left)
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
				if (ignoreColor > 0)
				{
					color = mColors[mColors.size - 1];
					color.a *= mAlpha * tint.a;
				}
				else
				{
					color = tint * mColors[mColors.size - 1];
					color.a *= mAlpha;
				}
				item = color.GammaToLinearSpace();
				int j = 0;
				for (int num12 = mColors.size - 2; j < num12; j++)
				{
					color.a *= mColors[j].a;
				}
				if (gradient)
				{
					a = (gradientBottom * color).GammaToLinearSpace();
					b = (gradientTop * color).GammaToLinearSpace();
				}
				i--;
				continue;
			}
			int space = 0;
			bool flag2 = italic > 0 || _fontStyle == FontStyle.Italic || _fontStyle == FontStyle.BoldAndItalic;
			bool flag3;
			if (encoding && ParseSpace(text, size2, ref i, out space))
			{
				flag3 = false;
				num = 32;
			}
			else
			{
				flag3 = bold > 0 || _fontStyle == FontStyle.Bold || _fontStyle == FontStyle.BoldAndItalic;
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
					val = ((float)size2 + spacingY) * fontScale;
					i--;
					continue;
				}
			}
			maxLineHeight.Set(val);
			BMSymbol bMSymbol = ((!useSymbols) ? null : GetSymbol(text, i, length));
			float num13;
			float num14;
			float y;
			float num15;
			if (bMSymbol != null)
			{
				num13 = num2 + (float)bMSymbol.offsetX * fontScale;
				num14 = num13 + (float)bMSymbol.width * fontScale;
				num15 = 0f - (num3 + (float)bMSymbol.offsetY * fontScale);
				y = num15 - (float)bMSymbol.height * fontScale;
				if (Mathf.RoundToInt(num2 + (float)bMSymbol.advance * fontScale) > regionWidth)
				{
					if (num2 == 0f)
					{
						return;
					}
					if (alignment != NGUIText.Alignment.Left && size < verts.size)
					{
						Align(verts, size, num2 - finalSpacingX);
						size = verts.size;
					}
					num13 -= num2;
					num14 -= num2;
					y -= maxLineHeight;
					num15 -= maxLineHeight;
					num2 = 0f;
					num3 += maxLineHeight;
					num11 = 0f;
					num10 = verts.size;
				}
				verts.Add(new Vector3(num13, y));
				verts.Add(new Vector3(num13, num15));
				verts.Add(new Vector3(num14, num15));
				verts.Add(new Vector3(num14, y));
				num2 += finalSpacingX + (float)bMSymbol.advance * fontScale;
				i += bMSymbol.length - 1;
				prev = 0;
				if (uvs != null)
				{
					Rect uvRect = bMSymbol.uvRect;
					float xMin = uvRect.xMin;
					float yMin = uvRect.yMin;
					float xMax = uvRect.xMax;
					float yMax = uvRect.yMax;
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
				float num16 = baseline + ((float)finalSize - baseline) * (float)size2 / (float)finalSize;
				glyphInfo.v0 = new Vector2(0f, ((float)size2 - num16) * fontScale);
				glyphInfo.v1 = new Vector2(space, (0f - num16) * fontScale);
				glyphInfo.u0 = Vector2.zero;
				glyphInfo.u1 = Vector2.zero;
				glyphInfo.u2 = Vector2.zero;
				glyphInfo.u3 = Vector2.zero;
				glyphInfo.advance = space;
			}
			if (sub != 0)
			{
				glyphInfo.v0.x *= 0.75f;
				glyphInfo.v0.y *= 0.75f;
				glyphInfo.v1.x *= 0.75f;
				glyphInfo.v1.y *= 0.75f;
				if (sub == 1)
				{
					glyphInfo.v0.y -= fontScale * (float)size2 * 0.4f;
					glyphInfo.v1.y -= fontScale * (float)size2 * 0.4f;
				}
				else
				{
					glyphInfo.v0.y += fontScale * (float)size2 * 0.05f;
					glyphInfo.v1.y += fontScale * (float)size2 * 0.05f;
				}
			}
			num13 = glyphInfo.v0.x + num2;
			y = glyphInfo.v0.y - num3;
			num14 = glyphInfo.v1.x + num2;
			num15 = glyphInfo.v1.y - num3;
			float num17 = glyphInfo.advance;
			if (finalSpacingX < 0f)
			{
				num17 += finalSpacingX;
			}
			if (Mathf.RoundToInt(num2 + num17) > regionWidth)
			{
				if (num2 == 0f)
				{
					return;
				}
				if (alignment != NGUIText.Alignment.Left && size < verts.size)
				{
					Align(verts, size, num2 - finalSpacingX);
					size = verts.size;
				}
				num13 -= num2;
				num14 -= num2;
				y -= maxLineHeight;
				num15 -= maxLineHeight;
				num2 = 0f;
				num3 += maxLineHeight;
				num11 = 0f;
				num10 = verts.size;
			}
			if (IsSpace(num))
			{
				if (underline > 0)
				{
					num = 95;
				}
				else if (strike > 0)
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
				if (bitmapFont != null)
				{
					glyphInfo.u0.x = rect.xMin + num6 * glyphInfo.u0.x;
					glyphInfo.u2.x = rect.xMin + num6 * glyphInfo.u2.x;
					glyphInfo.u0.y = rect.yMax - num7 * glyphInfo.u0.y;
					glyphInfo.u2.y = rect.yMax - num7 * glyphInfo.u2.y;
					glyphInfo.u1.x = glyphInfo.u0.x;
					glyphInfo.u1.y = glyphInfo.u2.y;
					glyphInfo.u3.x = glyphInfo.u2.x;
					glyphInfo.u3.y = glyphInfo.u0.y;
				}
				int m = 0;
				for (int num18 = ((!flag3) ? 1 : 4); m < num18; m++)
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
						float num19 = num8 + glyphInfo.v0.y / fontScale;
						float num20 = num8 + glyphInfo.v1.y / fontScale;
						num19 /= num8;
						num20 /= num8;
						s_c0 = Color.Lerp(a, b, num19);
						s_c1 = Color.Lerp(a, b, num20);
						int n = 0;
						for (int num21 = ((!flag3) ? 1 : 4); n < num21; n++)
						{
							cols.Add(s_c0);
							cols.Add(s_c1);
							cols.Add(s_c1);
							cols.Add(s_c0);
						}
					}
					else
					{
						int num22 = 0;
						for (int num23 = ((!flag3) ? 4 : 16); num22 < num23; num22++)
						{
							cols.Add(item);
						}
					}
				}
				else
				{
					Color c = color;
					c *= 0.49f;
					switch (glyphInfo.channel)
					{
					case 1:
						c.b += 0.51f;
						break;
					case 2:
						c.g += 0.51f;
						break;
					case 4:
						c.r += 0.51f;
						break;
					case 8:
						c.a += 0.51f;
						break;
					}
					Color item2 = c.GammaToLinearSpace();
					int num24 = 0;
					for (int num25 = ((!flag3) ? 4 : 16); num24 < num25; num24++)
					{
						cols.Add(item2);
					}
				}
			}
			if (!flag3)
			{
				if (!flag2)
				{
					verts.Add(new Vector3(num13, y));
					verts.Add(new Vector3(num13, num15));
					verts.Add(new Vector3(num14, num15));
					verts.Add(new Vector3(num14, y));
				}
				else
				{
					float num26 = (float)size2 * 0.1f * ((num15 - y) / (float)size2);
					verts.Add(new Vector3(num13 - num26, y));
					verts.Add(new Vector3(num13 + num26, num15));
					verts.Add(new Vector3(num14 + num26, num15));
					verts.Add(new Vector3(num14 - num26, y));
				}
			}
			else
			{
				for (int num27 = 0; num27 < 4; num27++)
				{
					float num28 = mBoldOffset[num27 * 2];
					float num29 = mBoldOffset[num27 * 2 + 1];
					float num30 = ((!flag2) ? 0f : ((float)size2 * 0.1f * ((num15 - y) / (float)size2)));
					verts.Add(new Vector3(num13 + num28 - num30, y + num29));
					verts.Add(new Vector3(num13 + num28 + num30, num15 + num29));
					verts.Add(new Vector3(num14 + num28 + num30, num15 + num29));
					verts.Add(new Vector3(num14 + num28 - num30, y + num29));
				}
			}
			if (underline <= 0 && strike <= 0)
			{
				continue;
			}
			GlyphInfo glyphInfo2 = GetGlyph((strike <= 0) ? 95 : 45, prev, size2);
			if (glyphInfo2 == null)
			{
				continue;
			}
			if (uvs != null)
			{
				if (bitmapFont != null)
				{
					glyphInfo2.u0.x = rect.xMin + num6 * glyphInfo2.u0.x;
					glyphInfo2.u2.x = rect.xMin + num6 * glyphInfo2.u2.x;
					glyphInfo2.u0.y = rect.yMax - num7 * glyphInfo2.u0.y;
					glyphInfo2.u2.y = rect.yMax - num7 * glyphInfo2.u2.y;
				}
				float x = (glyphInfo2.u0.x + glyphInfo2.u2.x) * 0.5f;
				int num31 = 0;
				for (int num32 = ((!flag3) ? 1 : 4); num31 < num32; num31++)
				{
					uvs.Add(new Vector2(x, glyphInfo2.u0.y));
					uvs.Add(new Vector2(x, glyphInfo2.u2.y));
					uvs.Add(new Vector2(x, glyphInfo2.u2.y));
					uvs.Add(new Vector2(x, glyphInfo2.u0.y));
				}
			}
			if (flag && strike > 0)
			{
				y = (0f - num3 + glyphInfo2.v0.y) * 0.75f;
				num15 = (0f - num3 + glyphInfo2.v1.y) * 0.75f;
			}
			else
			{
				y = 0f - num3 + glyphInfo2.v0.y;
				num15 = 0f - num3 + glyphInfo2.v1.y;
			}
			if (flag3)
			{
				for (int num33 = 0; num33 < 4; num33++)
				{
					float num34 = mBoldOffset[num33 * 2];
					float num35 = mBoldOffset[num33 * 2 + 1];
					verts.Add(new Vector3(num11 + num34, y + num35));
					verts.Add(new Vector3(num11 + num34, num15 + num35));
					verts.Add(new Vector3(num2 + num34, num15 + num35));
					verts.Add(new Vector3(num2 + num34, y + num35));
				}
			}
			else
			{
				verts.Add(new Vector3(num11, y));
				verts.Add(new Vector3(num11, num15));
				verts.Add(new Vector3(num2, num15));
				verts.Add(new Vector3(num2, y));
			}
			if (gradient)
			{
				float num36 = num8 + glyphInfo2.v0.y / fontScale;
				float num37 = num8 + glyphInfo2.v1.y / fontScale;
				num36 /= num8;
				num37 /= num8;
				s_c0 = Color.Lerp(a, b, num36);
				s_c1 = Color.Lerp(a, b, num37);
				int num38 = 0;
				for (int num39 = ((!flag3) ? 1 : 4); num38 < num39; num38++)
				{
					cols.Add(s_c0);
					cols.Add(s_c1);
					cols.Add(s_c1);
					cols.Add(s_c0);
				}
			}
			else
			{
				int num40 = 0;
				for (int num41 = ((!flag3) ? 4 : 16); num40 < num41; num40++)
				{
					cols.Add(item);
				}
			}
		}
		ChangeBaseline(verts, start, verts.size, maxLineHeight.Get());
		if (alignment != NGUIText.Alignment.Left && size < verts.size)
		{
			Align(verts, size, num2 - finalSpacingX);
			size = verts.size;
		}
		mColors.Clear();
	}

	private static void ChangeBaseline(BetterList<Vector3> verts, int start, int end, float height)
	{
		if (finalLineHeight != height)
		{
			float num = baseline * (finalLineHeight - height) / (float)finalSize;
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
				if (alignment != NGUIText.Alignment.Left)
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
					if (alignment != NGUIText.Alignment.Left && size < verts.size)
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
				if (alignment != NGUIText.Alignment.Left && size < verts.size)
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
		if (alignment != NGUIText.Alignment.Left && size < verts.size)
		{
			Align(verts, size, num - finalSpacingX, 1);
		}
	}

	public static void PrintExactCharacterPositions(string text, BetterList<Vector3> verts, BetterList<int> indices)
	{
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
				if (alignment != NGUIText.Alignment.Left)
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
					if (alignment != NGUIText.Alignment.Left && size < verts.size)
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
				if (alignment != NGUIText.Alignment.Left && size < verts.size)
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
		if (alignment != NGUIText.Alignment.Left && size < verts.size)
		{
			Align(verts, size, num2 - finalSpacingX, 2);
		}
	}

	public static void PrintCaretAndSelection(string text, int start, int end, BetterList<Vector3> caret, BetterList<Vector3> highlight)
	{
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
		Vector2 vector = Vector2.zero;
		Vector2 vector2 = Vector2.zero;
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
					if (alignment != NGUIText.Alignment.Left)
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
						highlight.Add(vector2);
						highlight.Add(vector);
					}
					else if (start <= i && end > i)
					{
						highlight.Add(new Vector3(num2, 0f - num3 - num5));
						highlight.Add(new Vector3(num2, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3 - num5));
					}
					if (alignment != NGUIText.Alignment.Left && num6 < highlight.size)
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
					if (alignment != NGUIText.Alignment.Left)
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
						highlight.Add(vector2);
						highlight.Add(vector);
					}
					else if (start <= i && end > i)
					{
						highlight.Add(new Vector3(num2, 0f - num3 - num5));
						highlight.Add(new Vector3(num2, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3));
						highlight.Add(new Vector3(num2 + 2f, 0f - num3 - num5));
					}
					if (alignment != NGUIText.Alignment.Left && num6 < highlight.size)
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
						highlight.Add(vector2);
						highlight.Add(vector);
					}
				}
				else if (!flag)
				{
					flag = true;
					highlight.Add(new Vector3(num9, num11));
					highlight.Add(new Vector3(num9, num12));
				}
			}
			vector = new Vector2(num10, num11);
			vector2 = new Vector2(num10, num12);
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
			if (alignment != NGUIText.Alignment.Left)
			{
				Align(caret, indexOffset, num2 - finalSpacingX);
			}
		}
		if (highlight != null)
		{
			if (flag)
			{
				highlight.Add(vector2);
				highlight.Add(vector);
			}
			else if (start < i && end == i)
			{
				highlight.Add(new Vector3(num2, 0f - num3 - num5));
				highlight.Add(new Vector3(num2, 0f - num3));
				highlight.Add(new Vector3(num2 + 2f, 0f - num3));
				highlight.Add(new Vector3(num2 + 2f, 0f - num3 - num5));
			}
			if (alignment != NGUIText.Alignment.Left && num6 < highlight.size)
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
