using System.Collections.Generic;
using Shared.Encyclopedia;
using Yaml.Util;

namespace Yaml;

public class EncyclopediaItems : SingletonDict<EncyclopediaType, Dictionary<string, EncyclopediaItem>>
{
	public static EncyclopediaItem Get(EncyclopediaType type, string key)
	{
		if (SingletonDict<EncyclopediaType, Dictionary<string, EncyclopediaItem>>.TryGetValue(type, out var value))
		{
			return value.Get(key);
		}
		return null;
	}
}
