using System;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Durango.Utils.Extensions;

public static class StringExtensions
{
	public static bool ContainsIgnoreCase(this string source, string toCheck)
	{
		return source.IndexOf(toCheck, StringComparison.OrdinalIgnoreCase) >= 0;
	}

	public static float ToFloat(this string source, NumberStyles style = NumberStyles.Float | NumberStyles.AllowThousands)
	{
		float.TryParse(source, style, NumberFormatInfo.CurrentInfo, out var result);
		return result;
	}

	public static int ToInt(this string source, NumberStyles style = NumberStyles.Integer)
	{
		int.TryParse(source, style, NumberFormatInfo.CurrentInfo, out var result);
		return result;
	}

	public static long ToInt64(this string source, NumberStyles style = NumberStyles.Integer)
	{
		long.TryParse(source, style, NumberFormatInfo.CurrentInfo, out var result);
		return result;
	}

	public static Color ToColor(this string source)
	{
		return source.ToColor(Color.white);
	}

	public static Color ToColor(this string source, Color defaultColor)
	{
		return (source?.Length ?? 0) switch
		{
			6 => NGUIText.ParseColor24(source), 
			8 => NGUIText.ParseColor32(source, 0), 
			_ => defaultColor, 
		};
	}

	public static T ToEnum<T>(this string source, T value = default(T))
	{
		if (source.TryEnum<T>(out var value2, showError: true))
		{
			return value2;
		}
		return value;
	}

	public static bool TryEnum<T>(this string source, out T value, bool showError = false)
	{
		try
		{
			value = (T)Enum.Parse(typeof(T), source, ignoreCase: true);
			return true;
		}
		catch (Exception)
		{
			value = default(T);
			return false;
		}
	}

	public static string[] SplitAndTrim(this string source, char sep)
	{
		string[] array = source.Split(new char[1] { sep }, StringSplitOptions.RemoveEmptyEntries);
		for (int i = 0; i < array.Length; i++)
		{
			array[i] = array[i].Trim();
		}
		return array;
	}

	public static string RemoveFromEnd(this string source, string suffix)
	{
		if (source.EndsWith(suffix))
		{
			return source.Substring(0, source.Length - suffix.Length);
		}
		return source;
	}

	public static string RemoveFromBegin(this string source, string prefix)
	{
		if (source.StartsWith(prefix))
		{
			return source.Substring(prefix.Length, source.Length - prefix.Length);
		}
		return source;
	}

	public static string ToTitleCase(this string source)
	{
		return source.Substring(0, 1).ToUpper() + source.Substring(1).ToLower();
	}

	public static string ToCamelCase(this string source)
	{
		return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(source.Replace('_', ' ')).Replace(" ", string.Empty);
	}

	public static string ToSnakeCase(this string source)
	{
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		for (int i = 0; i < source.Length; i++)
		{
			char c = source[i];
			bool flag = false;
			if (i > 0 && char.IsUpper(c))
			{
				char c2 = source[i - 1];
				char? c3 = ((i + 1 >= source.Length) ? null : new char?(source[i + 1]));
				if (char.IsLower(c2))
				{
					flag = true;
				}
				else if (!c3.HasValue || char.IsLower(c3.Value))
				{
					flag = true;
				}
			}
			if (flag)
			{
				value.Append('_');
				value.Append(char.ToLower(c));
			}
			else
			{
				value.Append(char.ToLower(c));
			}
		}
		return value.ToString();
	}

	public static string AddPostfix(this string source, string postfix)
	{
		if (string.IsNullOrEmpty(source))
		{
			return string.Empty;
		}
		return source + postfix;
	}

	public static string AddPrefix(this string source, string prefix)
	{
		if (string.IsNullOrEmpty(source))
		{
			return string.Empty;
		}
		return prefix + source;
	}
}
