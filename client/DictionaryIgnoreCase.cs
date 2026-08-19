using System;
using System.Collections.Generic;

public class DictionaryIgnoreCase<T> : Dictionary<string, T>
{
	public DictionaryIgnoreCase()
		: base((IEqualityComparer<string>)StringComparer.OrdinalIgnoreCase)
	{
	}
}
