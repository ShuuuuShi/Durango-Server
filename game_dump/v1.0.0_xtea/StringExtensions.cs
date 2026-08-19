using System;
using System.Globalization;
using System.Runtime.InteropServices;

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

	public static T ToEnum<T>(this string source, [Optional] T value)
	{
		try
		{
			return (T)Enum.Parse(typeof(T), source, ignoreCase: true);
		}
		catch (Exception)
		{
			return value;
		}
	}

	public static bool TryEnum<T>(this string source, out T value)
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
}
