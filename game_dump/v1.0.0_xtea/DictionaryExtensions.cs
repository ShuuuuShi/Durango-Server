using System.Collections.Generic;
using System.Runtime.InteropServices;

public static class DictionaryExtensions
{
	public static TV Get<TK, TV>(this IDictionary<TK, TV> dict, TK key, [Optional] TV defaultValue)
	{
		TV value;
		return (!dict.TryGetValue(key, out value)) ? defaultValue : value;
	}
}
