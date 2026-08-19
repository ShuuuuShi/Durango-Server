using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class ListExtensions
{
	public static int IndexOfIgnoreCase(this IList<string> list, string value)
	{
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (list[i].Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOf(this IList<string> array, string value)
	{
		int count = array.Count;
		for (int i = 0; i < count; i++)
		{
			if (array[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool ContainsIgnoreCase(this IList<string> array, string value)
	{
		return array.IndexOfIgnoreCase(value) != -1;
	}

	public static bool Contains(this IList<string> array, string value)
	{
		return IndexOf(array, value) != -1;
	}

	public static int IndexOf(this IList<int> array, int value)
	{
		int count = array.Count;
		for (int i = 0; i < count; i++)
		{
			if (array[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool Contains(this IList<int> array, int value)
	{
		return IndexOf(array, value) != -1;
	}

	public static int IndexOf(this IList<byte> array, byte value)
	{
		int count = array.Count;
		for (int i = 0; i < count; i++)
		{
			if (array[i] == value)
			{
				return i;
			}
		}
		return -1;
	}

	public static bool Contains(this IList<byte> array, byte value)
	{
		return IndexOf(array, value) != -1;
	}

	public static bool Any<T>(this IList<T> array, Func<T, bool> predicate)
	{
		int count = array.Count;
		for (int i = 0; i < count; i++)
		{
			if (predicate(array[i]))
			{
				return true;
			}
		}
		return false;
	}

	public static T Get<T>(this IList<T> list, int index, [Optional] T defaultValue)
	{
		if (index < 0 || index >= list.Count)
		{
			return defaultValue;
		}
		return list[index];
	}
}
