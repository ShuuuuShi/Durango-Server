using System.Collections.Generic;

namespace Durango.Utils.Extensions;

public static class KeyValuePairExtension
{
	public static KeyValuePair<T, U> WithKey<T, U>(this KeyValuePair<T, U> source, T newKey)
	{
		return new KeyValuePair<T, U>(newKey, source.Value);
	}

	public static KeyValuePair<T, U> WithValue<T, U>(this KeyValuePair<T, U> source, U newValue)
	{
		return new KeyValuePair<T, U>(source.Key, newValue);
	}
}
