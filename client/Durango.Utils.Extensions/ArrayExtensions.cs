using System;
using System.Text;
using UnityEngine;

namespace Durango.Utils.Extensions;

public static class ArrayExtensions
{
	public static int IndexOf<T>(this T[] source, T value)
	{
		return Array.IndexOf(source, value);
	}

	public static bool Contains<T>(this T[] source, T value)
	{
		return source.IndexOf(value) != -1;
	}

	public static int IndexOfIgnoreCase(this string[] source, string value)
	{
		int num = source.Length;
		for (int i = 0; i < num; i++)
		{
			if (source[i].Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	public static bool ContainsIgnoreCase(this string[] source, string value)
	{
		return source.IndexOfIgnoreCase(value) != -1;
	}

	public static int IndexOf<T>(this T[] source, Predicate<T> predicate)
	{
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		int num = source.Length;
		for (int i = 0; i < num; i++)
		{
			if (predicate(source[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static bool TryGet<T>(this T[] source, int index, out T element)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (index >= 0 && index < source.Length)
		{
			element = source[index];
			return true;
		}
		element = default(T);
		return false;
	}

	public static T Get<T>(this T[] source, int index, T defaultValue = default(T))
	{
		if (index < 0 || index >= source.Length)
		{
			return defaultValue;
		}
		return source[index];
	}

	public static void SetAll<T>(this T[] source, T value)
	{
		int num = source.Length;
		for (int i = 0; i < num; i++)
		{
			source[i] = value;
		}
	}

	public static T Random<T>(this T[] source, global::System.Random random = null)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (source.Length == 0)
		{
			throw Error.NoElements();
		}
		return source[random?.Next(0, source.Length) ?? UnityEngine.Random.Range(0, source.Length)];
	}

	public static void Shuffle<T>(this T[] source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		int num = source.Length;
		while (num > 1)
		{
			num--;
			int num2 = UnityEngine.Random.Range(0, num + 1);
			T val = source[num2];
			source[num2] = source[num];
			source[num] = val;
		}
	}

	public static string AsString<T>(this T[] source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		value.Append("{ ");
		for (int i = 0; i < source.Length; i++)
		{
			if (i != 0)
			{
				value.Append(", ");
			}
			value.Append(source[i]);
		}
		value.Append(" }");
		return value.ToString();
	}
}
