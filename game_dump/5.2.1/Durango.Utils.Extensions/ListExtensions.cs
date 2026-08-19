using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Durango.Utils.Extensions;

public static class ListExtensions
{
	public static int IndexOfIgnoreCase(this IList<string> source, string value)
	{
		int count = source.Count;
		for (int i = 0; i < count; i++)
		{
			if (source[i].Equals(value, StringComparison.OrdinalIgnoreCase))
			{
				return i;
			}
		}
		return -1;
	}

	public static int IndexOf<T>(this IList<T> source, Predicate<T> predicate)
	{
		if (predicate == null)
		{
			throw Error.ArgumentNull("predicate");
		}
		int count = source.Count;
		for (int i = 0; i < count; i++)
		{
			if (predicate(source[i]))
			{
				return i;
			}
		}
		return -1;
	}

	public static T Get<T>(this IList<T> source, int index, T defaultValue = default(T))
	{
		if (index < 0 || index >= source.Count)
		{
			return defaultValue;
		}
		return source[index];
	}

	public static void SetAll<T>(this IList<T> source, T value)
	{
		for (int i = 0; i < source.Count; i++)
		{
			source[i] = value;
		}
	}

	public static T Random<T>(this IList<T> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (source.Count == 0)
		{
			throw Error.NoElements();
		}
		return source[UnityEngine.Random.Range(0, source.Count)];
	}

	public static IList<T> Shuffle<T>(this IList<T> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		int num = source.Count;
		while (num > 1)
		{
			num--;
			int index = UnityEngine.Random.Range(0, num + 1);
			T value = source[index];
			source[index] = source[num];
			source[num] = value;
		}
		return source;
	}

	public static IList<T> ShuffleTake<T>(this IList<T> source, int count)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		List<T> list = new List<T>();
		int num = count;
		int num2 = -1;
		IList<T> source2 = source.ToList();
		IList<T> list2 = null;
		do
		{
			if (num2 < 0)
			{
				list2 = source2.Shuffle();
				num2 = list2.Count - 1;
			}
			list.Add(list2[num2]);
			num--;
			num2--;
		}
		while (num > 0);
		return list;
	}

	public static string AsString<T>(this IList<T> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		value.Append("{ ");
		for (int i = 0; i < source.Count; i++)
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

	public static List<List<TKey>> Split<TKey>(this IList<TKey> source, int splitCount)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (splitCount <= 0)
		{
			throw Error.ArgumentNull("invalid split count");
		}
		List<List<TKey>> list = new List<List<TKey>>();
		if (source.Count == 0)
		{
			return list;
		}
		int i = 0;
		for (int num = source.Count / splitCount + 1; i < num && i * splitCount != source.Count; i++)
		{
			list.Add(new List<TKey>());
			int j = 0;
			for (int num2 = (((i + 1) * splitCount > source.Count) ? (source.Count % splitCount) : splitCount); j < num2; j++)
			{
				list[i].Add(source[i * splitCount + j]);
			}
		}
		return list;
	}

	public static List<T> Fill<T>(this List<T> source, Func<T> defaultObj, int count)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		while (source.Count < count)
		{
			source.Add(default(T));
		}
		return source;
	}

	public static TSource MinBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		comparer = comparer ?? Comparer<TKey>.Default;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw new InvalidOperationException("Sequence contains no elements");
		}
		TSource val = enumerator.Current;
		TKey y = selector(val);
		while (enumerator.MoveNext())
		{
			TSource current = enumerator.Current;
			TKey val2 = selector(current);
			if (comparer.Compare(val2, y) < 0)
			{
				val = current;
				y = val2;
			}
		}
		return val;
	}

	public static TSource MaxBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> selector, IComparer<TKey> comparer = null)
	{
		if (source == null)
		{
			throw new ArgumentNullException("source");
		}
		if (selector == null)
		{
			throw new ArgumentNullException("selector");
		}
		comparer = comparer ?? Comparer<TKey>.Default;
		using IEnumerator<TSource> enumerator = source.GetEnumerator();
		if (!enumerator.MoveNext())
		{
			throw new InvalidOperationException("Sequence contains no elements");
		}
		TSource val = enumerator.Current;
		TKey y = selector(val);
		while (enumerator.MoveNext())
		{
			TSource current = enumerator.Current;
			TKey val2 = selector(current);
			if (comparer.Compare(val2, y) > 0)
			{
				val = current;
				y = val2;
			}
		}
		return val;
	}
}
