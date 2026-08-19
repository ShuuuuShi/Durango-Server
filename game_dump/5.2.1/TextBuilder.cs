using System;
using System.Collections.Generic;
using System.Text;
using Durango.Utils;
using UnityEngine;

public class TextBuilder : IDisposable
{
	public delegate bool TextParseDelegate(string str, ref int index, TextBuilder builder, TextTokens tokens);

	public class GlyphInfo
	{
		public Vector2 v0;

		public Vector2 v1;

		public Vector2 u0;

		public Vector2 u1;

		public Vector2 u2;

		public Vector2 u3;

		public float advance;

		public float bearing;

		public int channel;
	}

	public class TextTokens
	{
		public readonly List<TextToken> Tokens = new List<TextToken>();

		public readonly List<TextOption> Options = new List<TextOption>();

		public TextToken this[int index]
		{
			get
			{
				return Tokens[index];
			}
			set
			{
				Tokens[index] = value;
			}
		}

		public int Count => Tokens.Count;

		public TextOption LastOption => Options[Options.Count - 1];

		public void Clear()
		{
			Tokens.Clear();
			Options.Clear();
		}

		public void Add(TextToken token)
		{
			Tokens.Add(token);
		}

		public void Add(TextOption option)
		{
			int num = (option.Index = Tokens.Count);
			if (Options.Count > 0 && Options[Options.Count - 1].Index == num)
			{
				Options[Options.Count - 1] = option;
			}
			else
			{
				Options.Add(option);
			}
		}

		public bool IsEmpty()
		{
			if (Tokens.Count == 0)
			{
				return Options.Count == 0;
			}
			return false;
		}

		public bool IsValid()
		{
			if (Tokens.Count != 0)
			{
				return Options.Count != 0;
			}
			return false;
		}

		public string ToRawText()
		{
			StringBuilder stringBuilder = new StringBuilder();
			foreach (TextToken token in Tokens)
			{
				switch (token.Type)
				{
				case TokenType.Line:
					stringBuilder.AppendLine();
					break;
				case TokenType.Character:
					stringBuilder.Append((char)token.Character);
					break;
				case TokenType.Space:
				case TokenType.Link:
					stringBuilder.Append(' ');
					break;
				}
			}
			return stringBuilder.ToString();
		}
	}

	public struct TextToken
	{
		public TokenType Type;

		public Vector2 Size;

		public float Offset;

		public bool IsSingle;

		public int Character;

		public int PrevCharacter;

		public ITextRectLayout Link;

		public bool IsLineSeperator()
		{
			if (Type != TokenType.Space)
			{
				return Character == 45;
			}
			return true;
		}
	}

	public struct TextOption
	{
		public int Index;

		public int FontSize;

		public float Alignment;

		public Color Color;

		public Effects Effects;
	}

	[Flags]
	public enum Effects
	{
		None = 0,
		Bold = 2,
		Italic = 4,
		Underline = 8,
		Strikethrough = 0x10,
		Sub = 0x20,
		Sup = 0x40,
		IgnoreColor = 0x80
	}

	public enum TokenType
	{
		Line,
		Character,
		Space,
		Link
	}

	public const int Unlimit = 1000000;

	private static readonly float[] BoldOffset = new float[8] { -0.25f, 0f, 0.25f, 0f, 0f, -0.25f, 0f, 0.25f };

	private const float SizeShrinkage = 0.75f;

	public static bool WrapBySeperatorOnly = false;

	private static CharacterInfo mTempChar;

	public Font Font;

	public GlyphInfo Glyph = new GlyphInfo();

	public int FontSize = 16;

	public float FontScale = 1f;

	public FontStyle FontStyle;

	public float Alignment;

	public int Width = 1000000;

	public int Height = 1000000;

	public int MaxLines;

	public bool Gradient;

	public Color GradientBottom = Color.white;

	public Color GradientTop = Color.white;

	public bool Encoding;

	public float SpacingX;

	public float SpacingY;

	public bool Premultiply;

	private float _baseline;

	private readonly Stack<int> _fontSizeStack = new Stack<int>();

	private readonly BetterList<Color> _colors = new BetterList<Color>();

	private readonly BetterList<float> _alignments = new BetterList<float>();

	private static readonly Stack<TextBuilder> Pool = new Stack<TextBuilder>();

	private TextBuilder()
	{
	}

	public void Reset()
	{
		Font = null;
		FontSize = 16;
		FontScale = 1f;
		FontStyle = FontStyle.Normal;
		Alignment = 0f;
		Width = 1000000;
		Height = 1000000;
		MaxLines = 0;
		Gradient = false;
		GradientBottom = Color.white;
		GradientTop = Color.white;
		Encoding = false;
		SpacingX = 0f;
		SpacingY = 0f;
		Premultiply = false;
	}

	public void Update(bool request)
	{
		Font font = Font;
		if (!(font != null) || !request)
		{
			return;
		}
		font.RequestCharactersInTexture(")_-. ", FontSize);
		if (!font.GetCharacterInfo(')', out mTempChar, FontSize) || (float)mTempChar.maxY == 0f)
		{
			font.RequestCharactersInTexture("A", FontSize);
			if (!font.GetCharacterInfo('A', out mTempChar, FontSize))
			{
				_baseline = 0f;
				return;
			}
		}
		_baseline = 1f - ((float)mTempChar.maxY + (float)(FontSize - mTempChar.glyphHeight) * 0.5f) / (float)FontSize;
	}

	private void Prepare(string text, TextParseDelegate parser)
	{
		Prepare(text, FontSize, parser);
	}

	private void Prepare(string text, int fontSize, TextParseDelegate parser)
	{
		if (Font == null)
		{
			return;
		}
		if (Encoding)
		{
			_fontSizeStack.Clear();
			int length = text.Length;
			int size = fontSize;
			int size2 = size;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < length; i++)
			{
				num2 = i;
				if (parser != null && parser(text, ref i, this, null))
				{
					if (num < num2)
					{
						Font.RequestCharactersInTexture(text.Substring(num, num2 - num), size2);
					}
					num = i;
					i--;
				}
				else if (NGUIText.ParseSize(_fontSizeStack, text, ref i, ref size))
				{
					if (num < num2)
					{
						Font.RequestCharactersInTexture(text.Substring(num, num2 - num), size2);
					}
					size2 = size;
					num = i;
					i--;
				}
				else if (NGUIText.ParseSymbol(text, ref i))
				{
					if (num < num2)
					{
						Font.RequestCharactersInTexture(text.Substring(num, num2 - num), size2);
					}
					num = i;
					i--;
				}
			}
			num2++;
			if (num < num2)
			{
				Font.RequestCharactersInTexture(text.Substring(num, num2 - num), size);
			}
		}
		else
		{
			Font.RequestCharactersInTexture(text, fontSize);
		}
	}

	private GlyphInfo GetGlyph(int ch, int prev, int size)
	{
		if (Font != null && Font.GetCharacterInfo((char)ch, out mTempChar, size))
		{
			Glyph.v0.x = mTempChar.minX;
			Glyph.v1.x = mTempChar.maxX;
			Glyph.v0.y = mTempChar.maxY;
			Glyph.v1.y = mTempChar.minY;
			Glyph.u0 = mTempChar.uvTopLeft;
			Glyph.u1 = mTempChar.uvBottomLeft;
			Glyph.u2 = mTempChar.uvBottomRight;
			Glyph.u3 = mTempChar.uvTopRight;
			Glyph.advance = mTempChar.advance;
			Glyph.bearing = mTempChar.bearing;
			Glyph.channel = 0;
			Glyph.v0.x = Mathf.Round(Glyph.v0.x);
			Glyph.v0.y = Mathf.Round(Glyph.v0.y);
			Glyph.v1.x = Mathf.Round(Glyph.v1.x);
			Glyph.v1.y = Mathf.Round(Glyph.v1.y);
			float fontScale = FontScale;
			if (fontScale != 1f)
			{
				Glyph.v0 *= fontScale;
				Glyph.v1 *= fontScale;
				Glyph.advance *= fontScale;
			}
			return Glyph;
		}
		return null;
	}

	public int CalculateOffsetToFit(string text)
	{
		if (string.IsNullOrEmpty(text) || Width < 1)
		{
			return 0;
		}
		Prepare(text, null);
		int length = text.Length;
		int prev = 0;
		float num = SpacingX * FontScale;
		using Reusable<List<float>> reusable = ReusableList<float>.Pop();
		List<float> value = reusable.Value;
		for (int i = 0; i < length; i++)
		{
			int num2 = text[i];
			GlyphInfo glyph = GetGlyph(num2, prev, FontSize);
			if (glyph != null)
			{
				value.Add(num + glyph.advance * FontScale);
			}
			prev = num2;
		}
		float num3 = Width;
		int num4 = value.Count;
		while (num4 > 0 && num3 > 0f)
		{
			num3 -= value[--num4];
		}
		value.Clear();
		if (num3 < 0f)
		{
			num4++;
		}
		return num4;
	}

	public string GetEndOfLineThatFits(string text)
	{
		int length = text.Length;
		int num = CalculateOffsetToFit(text);
		return text.Substring(num, length - num);
	}

	public void ParseText(string text, TextTokens result, TextParseDelegate parser)
	{
		result.Clear();
		if (string.IsNullOrEmpty(text))
		{
			result.Add(default(TextOption));
			return;
		}
		int size = FontSize;
		_fontSizeStack.Clear();
		int length = text.Length;
		Prepare(text, parser);
		int sub = 0;
		int bold = 0;
		int italic = 0;
		int underline = 0;
		int strike = 0;
		int ignoreColor = 0;
		Color color = Color.white;
		float num = Alignment;
		_colors.Clear();
		_colors.Add(color);
		_alignments.Clear();
		_alignments.Add(num);
		Effects effects = Effects.None;
		switch (FontStyle)
		{
		case FontStyle.Bold:
			effects = Effects.Bold;
			bold++;
			break;
		case FontStyle.Italic:
			effects = Effects.Italic;
			italic++;
			break;
		case FontStyle.BoldAndItalic:
			effects = Effects.Bold | Effects.Italic;
			bold++;
			italic++;
			break;
		}
		result.Add(new TextOption
		{
			Color = color,
			Alignment = num,
			FontSize = size,
			Effects = effects
		});
		bool flag = parser != null;
		int i = 0;
		int num2 = 0;
		int num3 = 1;
		for (; i < length; i++)
		{
			char c = text[i];
			bool flag2 = false;
			if (c == '\t')
			{
				flag2 = true;
				c = ' ';
			}
			if (c == '\n')
			{
				if (num3 == MaxLines)
				{
					break;
				}
				num3++;
				result.Add(new TextToken
				{
					Type = TokenType.Line,
					Size = new Vector2(0f, size)
				});
			}
			if (Encoding)
			{
				if (flag && parser(text, ref i, this, result))
				{
					i--;
					continue;
				}
				bool flag3 = false;
				if (NGUIText.ParseSymbol(text, ref i, Premultiply, _colors, _alignments, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor))
				{
					num = _alignments[_alignments.size - 1];
					color = _colors[_colors.size - 1];
					int j = 0;
					for (int num4 = _colors.size - 2; j < num4; j++)
					{
						color.a *= _colors[j].a;
					}
					flag3 = true;
				}
				else if (NGUIText.ParseSize(_fontSizeStack, text, ref i, ref size))
				{
					flag3 = true;
				}
				if (flag3)
				{
					Effects effects2 = Effects.None;
					if (bold > 0)
					{
						effects2 |= Effects.Bold;
					}
					if (italic > 0)
					{
						effects2 |= Effects.Italic;
					}
					if (ignoreColor > 0)
					{
						effects2 |= Effects.IgnoreColor;
					}
					if (underline > 0)
					{
						effects2 |= Effects.Underline;
					}
					if (strike > 0)
					{
						effects2 |= Effects.Strikethrough;
					}
					switch (sub)
					{
					case 1:
						effects2 |= Effects.Sub;
						break;
					case 2:
						effects2 |= Effects.Sup;
						break;
					}
					result.Add(new TextOption
					{
						Alignment = num,
						Color = color,
						FontSize = size,
						Effects = effects2
					});
					i--;
					continue;
				}
			}
			GlyphInfo glyph = GetGlyph(c, num2, size);
			bool flag4 = NGUIText.IsSpace(c);
			if (glyph != null || flag4)
			{
				float num5 = glyph?.advance ?? 0f;
				if (sub != 0)
				{
					num5 *= 0.75f;
				}
				if (flag4)
				{
					result.Add(new TextToken
					{
						Type = TokenType.Space,
						Size = new Vector2((!flag2) ? num5 : (num5 * 4f), size)
					});
				}
				else
				{
					result.Add(new TextToken
					{
						Type = TokenType.Character,
						Character = c,
						PrevCharacter = num2,
						Size = new Vector2(num5, size)
					});
				}
				num2 = c;
			}
		}
	}

	public int ProcessText(TextTokens tokens, TextTokens result, out Vector2 printedSize, int minSize, bool useEllipsis = false, bool wrapAlways = false)
	{
		result.Clear();
		int fontSize = FontSize;
		int num = FontSize;
		printedSize = new Vector2(0f, (float)fontSize * FontScale);
		if (tokens.Count == 0 || Width < 1 || Height < 1)
		{
			return num;
		}
		int num2 = ((MaxLines <= 0) ? 1000000 : MaxLines);
		if (num2 == 0)
		{
			return num;
		}
		while (true)
		{
			float scale = (float)num / (float)FontSize;
			result.Clear();
			if (ProcessText(tokens, result, num == minSize, scale, num2, useEllipsis, wrapAlways) || num <= minSize)
			{
				break;
			}
			num--;
		}
		printedSize = ProcessedText(result, FontScale * (float)num / (float)FontSize);
		return num;
	}

	private bool ProcessText(TextTokens tokens, TextTokens result, bool full, float scale, int maxLineCount, bool useEllipsis, bool wrapAlways)
	{
		float num = 0f;
		float num2 = 0f;
		int num3 = 0;
		int num4 = 0;
		int num5 = 0;
		float num6 = 0f;
		float num7 = 0f;
		int num8 = 1;
		int num9 = 0;
		float num10 = FontScale * scale;
		bool flag = true;
		bool flag2 = false;
		bool flag3 = false;
		float num11 = SpacingX * num10;
		float num12 = SpacingY * num10;
		int i = 0;
		while (true)
		{
			bool flag4 = num9 >= tokens.Count;
			if (flag4 || flag2)
			{
				float num13 = num6 - num7;
				float num14 = num2 + num13 + ((!(num2 > 0f)) ? 0f : num12);
				if (num14 <= (float)Height)
				{
					if (num8 == maxLineCount || (num9 <= num3 && num4 <= num3 && num5 <= num4))
					{
						flag4 = true;
						flag = num9 >= tokens.Count;
					}
					if (flag4)
					{
						num4 = num9;
					}
					if (num4 <= num3)
					{
						num4 = num9;
						if (flag3 && WrapBySeperatorOnly && !wrapAlways)
						{
							flag4 = true;
							flag = false;
						}
					}
					TextToken textToken = default(TextToken);
					textToken.Type = TokenType.Line;
					TextToken token = textToken;
					num = 0f;
					num2 = num14;
					num6 = 0f;
					num7 = 0f;
					for (; i < tokens.Options.Count; i++)
					{
						TextOption option = tokens.Options[i];
						if (option.Index > num3)
						{
							break;
						}
						result.Add(option);
					}
					result.Add(token);
					int? num15 = null;
					for (int j = num3; j < num4; j++)
					{
						if (!num15.HasValue)
						{
							num15 = ((i >= tokens.Options.Count) ? (-1) : tokens.Options[i].Index);
						}
						if (j == num15.Value)
						{
							result.Add(tokens.Options[i]);
							i++;
							num15 = null;
						}
						TextToken token2 = tokens[j];
						result.Add(token2);
					}
				}
				else
				{
					flag4 = true;
					flag = false;
				}
				if (flag4)
				{
					break;
				}
				if (num5 < num4)
				{
					num5 = num4;
				}
				num9 = (num3 = num5);
				flag2 = false;
				flag3 = false;
				num8++;
				continue;
			}
			TextToken textToken2 = tokens[num9];
			TokenType type = textToken2.Type;
			Vector2 vector = textToken2.Size * num10;
			float num16 = textToken2.Offset * num10;
			if (type == TokenType.Line)
			{
				num4 = num9;
				num5 = num9 + 1;
				flag2 = true;
				flag3 = false;
				continue;
			}
			if (textToken2.IsSingle)
			{
				if (num > 0f)
				{
					flag2 = true;
					flag3 = false;
					num4 = num9;
					continue;
				}
				if (num9 + 1 < tokens.Count && tokens[num9 + 1].Type != 0)
				{
					flag2 = true;
					num4 = num9 + 1;
					flag3 = false;
					continue;
				}
			}
			if (textToken2.IsLineSeperator())
			{
				num4 = ((textToken2.Type != TokenType.Space) ? (num9 + 1) : num9);
				num5 = num9 + 1;
			}
			if (num + vector.x > (float)Width)
			{
				if (full && Mathf.Round(num2 + (vector.y - num16) + num12 + (num6 - num7)) > (float)Height)
				{
					num4 = num9;
				}
				flag2 = true;
				flag3 = true;
				continue;
			}
			num6 = Mathf.Max(vector.y + num16, num6);
			num7 = Mathf.Min(num16, num7);
			if (vector.x > 0f)
			{
				num += vector.x + ((!(num > 0f)) ? 0f : num11);
			}
			if (type == TokenType.Link)
			{
				num4 = num9 + 1;
			}
			num9++;
		}
		if (!flag && useEllipsis && full && result.Count > 0 && tokens.Options.Count > 0)
		{
			TextOption lastOption = tokens.LastOption;
			Font.RequestCharactersInTexture(".", lastOption.FontSize);
			GlyphInfo glyph = GetGlyph(46, 46, lastOption.FontSize);
			float num17 = glyph.advance * num10 * 3f + num11 * 2f;
			if (num17 < (float)Width)
			{
				int num18 = -1;
				for (int num19 = result.Count - 1; num19 >= 0; num19--)
				{
					TextToken textToken3 = result[num19];
					if (textToken3.Type == TokenType.Line)
					{
						break;
					}
					num17 -= (textToken3.Size * num10).x + num11;
					if (!(num17 > 0f))
					{
						num18 = num19;
						break;
					}
				}
				if (num18 != -1)
				{
					result.Tokens.RemoveRange(num18, result.Count - num18);
					TextToken textToken4 = default(TextToken);
					textToken4.Type = TokenType.Character;
					textToken4.Character = 46;
					textToken4.PrevCharacter = 46;
					textToken4.Size = new Vector2(glyph.advance, lastOption.FontSize);
					TextToken token3 = textToken4;
					for (int k = 0; k < 3; k++)
					{
						result.Add(token3);
					}
				}
			}
		}
		return flag;
	}

	private Vector2 ProcessedText(TextTokens tokens, float scale)
	{
		float num = SpacingX * scale;
		float num2 = SpacingY * scale;
		int num3 = -1;
		Vector2 zero = Vector2.zero;
		do
		{
			float num4 = 0f;
			float num5 = 0f;
			float num6 = 0f;
			for (int i = num3 + 1; i < tokens.Count + 1; i++)
			{
				if (i >= tokens.Count || tokens[i].Type == TokenType.Line)
				{
					if (num3 >= 0 && num3 < tokens.Count)
					{
						TextToken value = tokens[num3];
						float num7 = num5 - num6;
						if (num7 <= 0f)
						{
							num7 = (float)tokens.LastOption.FontSize * scale;
						}
						value.Size = new Vector2(num4, num7);
						value.Offset = num6;
						tokens[num3] = value;
						zero.x = Mathf.Max(num4, zero.x);
						zero.y += ((!(zero.y > 0f)) ? num7 : (num7 + num2));
					}
					num3 = i;
					break;
				}
				TextToken value2 = tokens[i];
				Vector2 size = value2.Size * scale;
				float num8 = value2.Offset * scale;
				num4 += ((!(size.x > 0f)) ? size.x : (size.x + num));
				num5 = Mathf.Max(size.y + num8, num5);
				num6 = Mathf.Min(num8, num6);
				value2.Offset = num8;
				value2.Size = size;
				tokens[i] = value2;
			}
		}
		while (num3 < tokens.Count);
		if (zero.y <= 0f)
		{
			zero.y = (float)FontSize * scale;
		}
		return zero;
	}

	public void PrintCaretAndSelection(int width, TextTokens tokens, int start, int end, BetterList<Vector3> caret, BetterList<Vector3> highlight)
	{
		int num = end;
		if (start > end)
		{
			end = start;
			start = num;
		}
		float num2 = SpacingX * FontScale;
		Vector2 zero = Vector2.zero;
		bool flag = false;
		Vector2? vector = null;
		float num3 = (float)FontSize * FontScale;
		for (int i = 0; i < tokens.Count; i++)
		{
			TextToken textToken = tokens[i];
			if (textToken.Type == TokenType.Line)
			{
				if (highlight != null && vector.HasValue)
				{
					Vector2 value = vector.Value;
					vector = null;
					highlight.Add(new Vector3(value.x, value.y));
					highlight.Add(new Vector3(value.x, value.y + num3));
					highlight.Add(new Vector3(zero.x, value.y + num3));
					highlight.Add(new Vector3(zero.x, value.y));
				}
				num3 = textToken.Size.y;
				zero.x = ((float)width - textToken.Size.x) * Alignment;
				if (zero.y < 0f)
				{
					zero.y -= SpacingY * FontScale;
				}
				zero.y -= num3;
			}
			else
			{
				zero.x += textToken.Size.x + num2;
			}
			if (highlight != null)
			{
				if (!vector.HasValue && i >= start)
				{
					vector = zero;
				}
				if (vector.HasValue && i >= end)
				{
					Vector2 value2 = vector.Value;
					vector = null;
					highlight.Add(new Vector3(value2.x, value2.y));
					highlight.Add(new Vector3(value2.x, value2.y + num3));
					highlight.Add(new Vector3(zero.x, value2.y + num3));
					highlight.Add(new Vector3(zero.x, value2.y));
				}
			}
			Vector2 size = textToken.Size;
			if (caret != null && !flag && num <= i)
			{
				flag = true;
				caret.Add(new Vector3(zero.x - 1f, zero.y));
				caret.Add(new Vector3(zero.x - 1f, zero.y + size.y));
				caret.Add(new Vector3(zero.x + 1f, zero.y + size.y));
				caret.Add(new Vector3(zero.x + 1f, zero.y));
			}
		}
		if (zero.y >= 0f)
		{
			zero.y -= num3;
		}
		if (caret != null && !flag)
		{
			caret.Add(new Vector3(zero.x - 1f, zero.y));
			caret.Add(new Vector3(zero.x - 1f, zero.y + num3));
			caret.Add(new Vector3(zero.x + 1f, zero.y + num3));
			caret.Add(new Vector3(zero.x + 1f, zero.y));
		}
		if (highlight != null)
		{
			if (vector.HasValue)
			{
				Vector2 value3 = vector.Value;
				highlight.Add(new Vector3(value3.x, value3.y));
				highlight.Add(new Vector3(value3.x, value3.y + num3));
				highlight.Add(new Vector3(zero.x, value3.y + num3));
				highlight.Add(new Vector3(zero.x, value3.y));
			}
			else if (start < tokens.Count && end == tokens.Count)
			{
				highlight.Add(new Vector3(zero.x, zero.y));
				highlight.Add(new Vector3(zero.x, zero.y + num3));
				highlight.Add(new Vector3(zero.x + 2f, zero.y + num3));
				highlight.Add(new Vector3(zero.x + 2f, zero.y));
			}
		}
	}

	public void PrintApproximateCharacterPositions(int width, TextTokens tokens, BetterList<Vector3> verts, BetterList<int> indices)
	{
		if (tokens.Count == 0)
		{
			return;
		}
		float num = SpacingX * FontScale;
		Vector2 zero = Vector2.zero;
		for (int i = 0; i < tokens.Count; i++)
		{
			TextToken textToken = tokens[i];
			if (textToken.Type == TokenType.Line)
			{
				float y = textToken.Size.y;
				zero.x = ((float)width - textToken.Size.x) * Alignment;
				if (zero.y < 0f)
				{
					zero.y -= SpacingY * FontScale;
				}
				zero.y -= y;
			}
			else
			{
				zero.x += textToken.Size.x + num;
			}
			indices.Add(i);
			Vector2 size = textToken.Size;
			verts.Add(new Vector3(zero.x, zero.y + size.y * 0.5f));
		}
	}

	public static TextBuilder Pop()
	{
		TextBuilder obj = ((Pool.Count <= 0) ? new TextBuilder() : Pool.Pop());
		obj.Reset();
		return obj;
	}

	public void Dispose()
	{
		Reset();
		Pool.Push(this);
	}

	public void Build(TextTokens tokens, Color color, float width, BetterList<Vector3> verts, BetterList<Vector2> uvs, BetterList<Color> cols)
	{
		if (!tokens.IsValid())
		{
			return;
		}
		Vector2 vector = Vector3.zero;
		float num = 0f;
		float num2 = 0f;
		float num3 = SpacingX * FontScale;
		int num4 = 0;
		int num5 = 0;
		TextOption textOption = default(TextOption);
		int i = 0;
		for (int count = tokens.Count; i < count; i++)
		{
			TextToken textToken = tokens[i];
			float offset = textToken.Offset;
			if (num5 == i)
			{
				textOption = tokens.Options[num4];
				num4++;
				num5 = ((num4 >= tokens.Options.Count) ? (-1) : tokens.Options[num4].Index);
			}
			switch (textToken.Type)
			{
			case TokenType.Line:
			{
				float num33 = textToken.Size.y + Mathf.Min(textToken.Offset, 0f);
				float alignment = textOption.Alignment;
				vector.x = (width - textToken.Size.x) * alignment;
				if (vector.y < 0f)
				{
					vector.y -= SpacingY * FontScale;
				}
				num = vector.y - num33;
				num2 = num + num33 * _baseline;
				vector.y -= textToken.Size.y;
				continue;
			}
			case TokenType.Character:
			{
				if (verts == null)
				{
					break;
				}
				int character = textToken.Character;
				int prevCharacter = textToken.PrevCharacter;
				int fontSize = textOption.FontSize;
				GlyphInfo glyph = GetGlyph(character, prevCharacter, fontSize);
				if (glyph == null)
				{
					break;
				}
				Color color3 = color;
				if ((textOption.Effects & Effects.IgnoreColor) != 0)
				{
					color3 = Color.white;
					color3.a = color.a;
				}
				Color color4 = textOption.Color * color3;
				Color? color5 = null;
				if (Gradient)
				{
					color4 *= GradientTop;
					color5 = GradientBottom * color3;
				}
				float num6 = num2 + offset;
				bool flag = (textOption.Effects & Effects.Sub) != 0;
				bool flag2 = (textOption.Effects & Effects.Sup) != 0;
				if (flag || flag2)
				{
					glyph.v0.x *= 0.75f;
					glyph.v0.y *= 0.75f;
					glyph.v1.x *= 0.75f;
					glyph.v1.y *= 0.75f;
					if (!flag)
					{
						float num7 = FontScale * (float)fontSize * 0.25f;
						glyph.v0.y += num7;
						glyph.v1.y += num7;
					}
				}
				float num8 = glyph.v0.x + vector.x;
				float num9 = glyph.v0.y + num6;
				float num10 = glyph.v1.x + vector.x;
				float num11 = glyph.v1.y + num6;
				bool flag3 = (textOption.Effects & Effects.Underline) != 0;
				bool flag4 = (textOption.Effects & Effects.Strikethrough) != 0;
				bool flag5 = (textOption.Effects & Effects.Bold) != 0;
				bool flag6 = (textOption.Effects & Effects.Italic) != 0;
				if (uvs != null)
				{
					int j = 0;
					for (int num12 = ((!flag5) ? 1 : 4); j < num12; j++)
					{
						uvs.Add(glyph.u0);
						uvs.Add(glyph.u1);
						uvs.Add(glyph.u2);
						uvs.Add(glyph.u3);
					}
				}
				if (cols != null)
				{
					if (glyph.channel == 0 || glyph.channel == 15)
					{
						if (color5.HasValue)
						{
							float num13 = (float)fontSize + glyph.v0.y / FontScale;
							float num14 = (float)fontSize + glyph.v1.y / FontScale;
							num13 /= (float)fontSize;
							num14 /= (float)fontSize;
							Color item = Color.Lerp(color4, color5.Value, num13);
							Color item2 = Color.Lerp(color4, color5.Value, num14);
							int k = 0;
							for (int num15 = ((!flag5) ? 1 : 4); k < num15; k++)
							{
								cols.Add(item);
								cols.Add(item2);
								cols.Add(item2);
								cols.Add(item);
							}
						}
						else
						{
							int l = 0;
							for (int num16 = ((!flag5) ? 4 : 16); l < num16; l++)
							{
								cols.Add(color4);
							}
						}
					}
					else
					{
						Color c = color4;
						c *= 0.49f;
						switch (glyph.channel)
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
						Color item3 = c.GammaToLinearSpace();
						int m = 0;
						for (int num17 = ((!flag5) ? 4 : 16); m < num17; m++)
						{
							cols.Add(item3);
						}
					}
				}
				if (!flag5)
				{
					if (!flag6)
					{
						verts.Add(new Vector3(num8, num9));
						verts.Add(new Vector3(num8, num11));
						verts.Add(new Vector3(num10, num11));
						verts.Add(new Vector3(num10, num9));
					}
					else
					{
						float num18 = (float)fontSize * 0.1f * ((num11 - num9) / (float)fontSize);
						verts.Add(new Vector3(num8 - num18, num9));
						verts.Add(new Vector3(num8 + num18, num11));
						verts.Add(new Vector3(num10 + num18, num11));
						verts.Add(new Vector3(num10 - num18, num9));
					}
				}
				else
				{
					for (int n = 0; n < 4; n++)
					{
						float num19 = BoldOffset[n * 2];
						float num20 = BoldOffset[n * 2 + 1];
						float num21 = ((!flag6) ? 0f : ((float)fontSize * 0.1f * ((num11 - num9) / (float)fontSize)));
						verts.Add(new Vector3(num8 + num19 - num21, num9 + num20));
						verts.Add(new Vector3(num8 + num19 + num21, num11 + num20));
						verts.Add(new Vector3(num10 + num19 + num21, num11 + num20));
						verts.Add(new Vector3(num10 + num19 - num21, num9 + num20));
					}
				}
				if (!flag3 && !flag4)
				{
					break;
				}
				GlyphInfo glyph2 = GetGlyph((!flag4) ? 95 : 45, prevCharacter, FontSize);
				if (glyph2 == null)
				{
					break;
				}
				if (uvs != null)
				{
					float x = (glyph2.u0.x + glyph2.u2.x) * 0.5f;
					int num22 = 0;
					for (int num23 = ((!flag5) ? 1 : 4); num22 < num23; num22++)
					{
						uvs.Add(new Vector2(x, glyph2.u0.y));
						uvs.Add(new Vector2(x, glyph2.u2.y));
						uvs.Add(new Vector2(x, glyph2.u2.y));
						uvs.Add(new Vector2(x, glyph2.u0.y));
					}
				}
				if (flag4)
				{
					num9 = num6 + glyph2.v0.y * 0.75f;
					num11 = num6 + glyph2.v1.y * 0.75f;
				}
				else
				{
					num9 = num6 + glyph2.v0.y;
					num11 = num6 + glyph2.v1.y;
				}
				if (flag5)
				{
					for (int num24 = 0; num24 < 4; num24++)
					{
						float num25 = BoldOffset[num24 * 2];
						float num26 = BoldOffset[num24 * 2 + 1];
						verts.Add(new Vector3(vector.x + num25, num9 + num26));
						verts.Add(new Vector3(vector.x + num25, num11 + num26));
						verts.Add(new Vector3(vector.x + textToken.Size.x + num25, num11 + num26));
						verts.Add(new Vector3(vector.x + textToken.Size.x + num25, num9 + num26));
					}
				}
				else
				{
					verts.Add(new Vector3(vector.x, num9));
					verts.Add(new Vector3(vector.x, num11));
					verts.Add(new Vector3(vector.x + textToken.Size.x, num11));
					verts.Add(new Vector3(vector.x + textToken.Size.x, num9));
				}
				if (color5.HasValue)
				{
					float num27 = (float)fontSize + glyph2.v0.y / FontScale;
					float num28 = (float)fontSize + glyph2.v1.y / FontScale;
					num27 /= (float)fontSize;
					num28 /= (float)fontSize;
					Color item4 = Color.Lerp(color4, color5.Value, num27);
					Color item5 = Color.Lerp(color4, color5.Value, num28);
					int num29 = 0;
					for (int num30 = ((!flag5) ? 1 : 4); num29 < num30; num29++)
					{
						cols.Add(item4);
						cols.Add(item5);
						cols.Add(item5);
						cols.Add(item4);
					}
				}
				else
				{
					int num31 = 0;
					for (int num32 = ((!flag5) ? 4 : 16); num31 < num32; num31++)
					{
						cols.Add(color4);
					}
				}
				break;
			}
			case TokenType.Link:
			{
				Color color2 = (((textOption.Effects & Effects.IgnoreColor) != 0) ? textOption.Color : (textOption.Color * color));
				color2.a = textOption.Color.a;
				textToken.Link.Set(new Vector2(vector.x, num + offset), textToken.Size, color2);
				break;
			}
			}
			vector.x += textToken.Size.x + num3;
		}
	}
}
