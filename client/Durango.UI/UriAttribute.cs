using System;

namespace Durango.UI;

public class UriAttribute : Attribute
{
	public string Key { get; private set; }

	public UriAttribute()
	{
	}

	public UriAttribute(string key)
	{
		Key = key;
	}
}
