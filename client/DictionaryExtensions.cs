using System.Collections.Generic;
using System.Text;
using Durango.Utils;
using Durango.Utils.Extensions;
using JetBrains.Annotations;

public static class DictionaryExtensions
{
	[CanBeNull]
	public static TV Get<TK, TV>(this IDictionary<TK, TV> dict, [CanBeNull] TK key, TV defaultValue = default(TV))
	{
		if (key == null)
		{
			return defaultValue;
		}
		TV value;
		return (!dict.TryGetValue(key, out value)) ? defaultValue : value;
	}

	public static bool TryGetValueWithSubStringKey<T>(this Dictionary<string, T> source, [NotNull] string key, out T value)
	{
		if (source.TryGetValue(key, out value))
		{
			return true;
		}
		foreach (KeyValuePair<string, T> item in source)
		{
			if (item.Key.Length > 0 && key.ContainsIgnoreCase(item.Key))
			{
				value = item.Value;
				return true;
			}
		}
		value = default(T);
		return false;
	}

	public static string AsString<TK, TV>(this IDictionary<TK, TV> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		value.Append("{\n");
		foreach (KeyValuePair<TK, TV> item in source)
		{
			value.Append("\t{ ");
			value.Append(item.Key);
			value.Append(", ");
			value.Append(item.Value);
			value.Append(" },\n");
		}
		value.Append("}");
		return value.ToString();
	}

	public static string AsString<T>(this IDictionary<string, List<T>> source)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		using Reusable<StringBuilder> reusable = ReusableStringBuilder.Pop();
		StringBuilder value = reusable.Value;
		value.Append("{\n");
		foreach (KeyValuePair<string, List<T>> item in source)
		{
			value.Append("\t{ ");
			value.Append(item.Key);
			value.Append(", ");
			value.Append(item.Value.AsString());
			value.Append(" },\n");
		}
		value.Append("}");
		return value.ToString();
	}

	public static void AddRange<TKey, TValue>(this IDictionary<TKey, TValue> source, IDictionary<TKey, TValue> target)
	{
		if (source == null)
		{
			throw Error.ArgumentNull("source");
		}
		if (target == null)
		{
			return;
		}
		foreach (KeyValuePair<TKey, TValue> item in target)
		{
			source[item.Key] = item.Value;
		}
	}
}
