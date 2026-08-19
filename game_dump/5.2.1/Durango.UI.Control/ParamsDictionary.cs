using System;
using System.Collections.Generic;
using Durango.Utils.Extensions;

namespace Durango.UI.Control;

public class ParamsDictionary : Dictionary<string, string>
{
	private ParamsDictionary()
		: base((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
	{
	}

	public static ParamsDictionary MakeParams(string text)
	{
		if (string.IsNullOrEmpty(text))
		{
			return null;
		}
		ParamsDictionary paramsDictionary = null;
		int num = 0;
		while (num < text.Length)
		{
			int num2 = text.IndexOf('=', num);
			if (num2 == -1 || !IsValidKey(text, num, num2 - num))
			{
				break;
			}
			string text2 = text.Substring(num, num2 - num);
			string text3 = null;
			num = num2 + 1;
			int num3 = -1;
			char value = '\0';
			for (int i = num; i < text.Length; i++)
			{
				char c = text[i];
				if (!char.IsWhiteSpace(c))
				{
					if (c == '"' || c == '\'')
					{
						value = c;
						num3 = i + 1;
					}
					break;
				}
			}
			if (num3 != -1)
			{
				int num4 = text.IndexOf(value, num3);
				if (num4 != -1)
				{
					text3 = text.Substring(num3, num4 - num3);
					int num5 = text.IndexOf(',', num4);
					num = ((num5 != -1) ? (num5 + 1) : text.Length);
				}
			}
			if (text3 == null)
			{
				int num6 = num;
				int num8;
				while (true)
				{
					int num7 = ((num6 >= text.Length) ? (-1) : text.IndexOf('=', num6));
					if (num7 == -1)
					{
						num8 = -1;
						break;
					}
					num8 = text.LastIndexOf(',', num7);
					if (num8 >= num)
					{
						break;
					}
					num6 = num7 + 1;
				}
				if (num8 == -1)
				{
					text3 = text.Substring(num, text.Length - num);
					num = text.Length;
				}
				else
				{
					text3 = text.Substring(num, num8 - num);
					num = num8 + 1;
				}
			}
			if (paramsDictionary == null)
			{
				paramsDictionary = new ParamsDictionary();
			}
			paramsDictionary.Add(text2.Trim(), text3.Trim());
		}
		return paramsDictionary;
	}

	private static bool IsValidKey(string text, int index, int length)
	{
		bool flag = true;
		bool flag2 = false;
		for (int i = 0; i < length; i++)
		{
			char c = text[index + i];
			if (flag)
			{
				if (!char.IsWhiteSpace(c))
				{
					if (!char.IsLetter(c) && c != '_')
					{
						return false;
					}
					flag = false;
				}
			}
			else if (char.IsWhiteSpace(c))
			{
				flag2 = true;
			}
			else
			{
				if (flag2)
				{
					return false;
				}
				if (!char.IsLetterOrDigit(c) && c != '_')
				{
					return false;
				}
			}
		}
		return true;
	}

	public float GetFloat(string key, float defaultValue = 0f)
	{
		if (TryGetValue(key, out var value) && !string.IsNullOrEmpty(value) && float.TryParse(value, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public int GetInt(string key, int defaultValue = 0)
	{
		if (TryGetValue(key, out var value) && !string.IsNullOrEmpty(value) && int.TryParse(value, out var result))
		{
			return result;
		}
		return defaultValue;
	}

	public T GetEnum<T>(string key, T defaultVavlue = default(T)) where T : struct
	{
		if (TryGetValue(key, out var value) && !string.IsNullOrEmpty(value) && value.TryEnum<T>(out var value2))
		{
			return value2;
		}
		return defaultVavlue;
	}
}
