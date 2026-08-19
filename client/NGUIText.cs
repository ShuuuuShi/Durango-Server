using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Durango.Utils;
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

	private static Color mInvisible = Color.clear;

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
		return ParseSymbol(text, ref index, premultiply: false, null, null, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor);
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public static bool IsHex(char ch)
	{
		return (ch >= '0' && ch <= '9') || (ch >= 'a' && ch <= 'f') || (ch >= 'A' && ch <= 'F');
	}

	public static bool ParseSize(Stack<int> stack, string text, ref int index, ref int size)
	{
		if (!string.IsNullOrEmpty(text) && text[index] == '[')
		{
			int length = text.Length;
			if (index + 7 > length)
			{
				return false;
			}
			if (stack.Count > 0 && text[index + 1] == '/' && text[index + 2] == 's' && text[index + 3] == 'i' && text[index + 4] == 'z' && text[index + 5] == 'e' && text[index + 6] == ']')
			{
				size = stack.Pop();
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
							stack.Push(size);
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
							stack.Push(size);
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
							stack.Push(size);
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
							stack.Push(size);
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
							stack.Push(size);
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

	public static bool ParseSymbol(string text, ref int index, bool premultiply, BetterList<Color> colors, BetterList<float> alignments, ref int sub, ref int bold, ref int italic, ref int underline, ref int strike, ref int ignoreColor)
	{
		int length = text.Length;
		if (index + 3 > length || text[index] != '[')
		{
			return false;
		}
		if (text[index + 2] == ']')
		{
			bool flag = false;
			switch (text[index + 1])
			{
			case 'b':
				bold++;
				flag = true;
				break;
			case 'i':
				italic++;
				flag = true;
				break;
			case 'u':
				underline++;
				flag = true;
				break;
			case 's':
				strike++;
				flag = true;
				break;
			case 'c':
				ignoreColor++;
				flag = true;
				break;
			case '-':
				if (colors != null && colors.size > 1)
				{
					colors.size--;
				}
				flag = true;
				break;
			}
			if (flag)
			{
				index += 3;
				return true;
			}
		}
		if (index + 4 > length)
		{
			return false;
		}
		if (text[index + 3] == ']' && text[index + 1] == '/')
		{
			bool flag2 = false;
			switch (text[index + 2])
			{
			case 'b':
				bold = Mathf.Max(bold - 1, 0);
				flag2 = true;
				break;
			case 'i':
				italic = Mathf.Max(italic - 1, 0);
				flag2 = true;
				break;
			case 'u':
				underline = Mathf.Max(underline - 1, 0);
				flag2 = true;
				break;
			case 's':
				strike = Mathf.Max(strike - 1, 0);
				flag2 = true;
				break;
			case 'c':
				ignoreColor = Mathf.Max(ignoreColor - 1, 0);
				flag2 = true;
				break;
			}
			if (flag2)
			{
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
			bool flag3 = false;
			if (text[index + 1] == 's' && text[index + 2] == 'u' && text[index + 3] == 'b')
			{
				sub = 1;
				flag3 = true;
			}
			else if (text[index + 1] == 's' && text[index + 2] == 'u' && text[index + 3] == 'p')
			{
				sub = 2;
				flag3 = true;
			}
			if (flag3)
			{
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
			bool flag4 = false;
			if (text[index + 1] == '/' && text[index + 2] == 's' && text[index + 3] == 'u' && text[index + 4] == 'b')
			{
				sub = 0;
				flag4 = true;
			}
			else if (text[index + 1] == '/' && text[index + 2] == 's' && text[index + 3] == 'u' && text[index + 4] == 'p')
			{
				sub = 0;
				flag4 = true;
			}
			else if (text[index + 1] == '/' && text[index + 2] == 'u' && text[index + 3] == 'r' && text[index + 4] == 'l')
			{
				flag4 = true;
			}
			if (flag4)
			{
				index += 6;
				return true;
			}
		}
		if (text[index + 1] == 'u' && text[index + 2] == 'r' && text[index + 3] == 'l' && text[index + 4] == '=')
		{
			int num = text.IndexOf(']', index + 4);
			if (num != -1)
			{
				index = num + 1;
				return true;
			}
			index = text.Length;
			return true;
		}
		if (index + 7 > length)
		{
			return false;
		}
		if (text[index + 1] == 'a' && text[index + 2] == 'l' && text[index + 3] == 'i' && text[index + 4] == 'g' && text[index + 5] == 'n' && text[index + 6] == '=')
		{
			int num2 = text.IndexOf(']', index + 7);
			if (num2 == -1)
			{
				return false;
			}
			if (float.TryParse(text.Substring(index + 7, num2 - (index + 7)), out var result))
			{
				alignments?.Add(result);
				index = num2 + 1;
				return true;
			}
		}
		if (index + 8 > length)
		{
			return false;
		}
		if (text[index + 7] == ']')
		{
			if (text[index + 1] == '/' && text[index + 2] == 'a' && text[index + 3] == 'l' && text[index + 4] == 'i' && text[index + 5] == 'g' && text[index + 6] == 'n')
			{
				if (alignments != null && alignments.size > 1)
				{
					alignments.size--;
				}
				index += 8;
				return true;
			}
			Color color = ParseColor24(text, index + 1);
			if (!IsColorEncoded(text, index + 1, 6))
			{
				return false;
			}
			if (colors != null)
			{
				color.a = colors[colors.size - 1].a;
				if (premultiply && color.a != 1f)
				{
					color = Color.Lerp(mInvisible, color, color.a);
				}
				colors.Add(color);
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
			Color color2 = ParseColor32(text, index + 1);
			if (!IsColorEncoded(text, index + 1, 8))
			{
				return false;
			}
			if (colors != null)
			{
				if (premultiply && color2.a != 1f)
				{
					color2 = Color.Lerp(mInvisible, color2, color2.a);
				}
				colors.Add(color2);
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
		if (string.IsNullOrEmpty(text))
		{
			return text;
		}
		using Reusable<Stack<int>> reusable = ReusableStack<int>.Pop();
		using Reusable<Stack<int>> reusable2 = ReusableStack<int>.Pop();
		Stack<int> value = reusable.Value;
		Stack<int> value2 = reusable2.Value;
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
				if (ParseSymbol(text, ref index, premultiply: false, null, null, ref sub, ref bold, ref italic, ref underline, ref strike, ref ignoreColor))
				{
					text = text.Remove(num, index - num);
					length = text.Length;
					if (value.Count > 0)
					{
						num = value.Pop();
					}
					continue;
				}
				int size = 0;
				if (ParseSize(value2, text, ref index, ref size))
				{
					text = text.Remove(num, index - num);
					length = text.Length;
					if (value.Count > 0)
					{
						num = value.Pop();
					}
					continue;
				}
				value.Push(num);
				break;
			}
			case ']':
				if (value.Count > 0)
				{
					value.Pop();
				}
				break;
			}
			num++;
		}
		return text;
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
	public static void ReplaceSpaceWithNewline(ref StringBuilder s)
	{
		int num = s.Length - 1;
		if (num > 0 && IsSpace(s[num]))
		{
			s[num] = '\n';
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
