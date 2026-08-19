using System.Collections.Generic;
using Yaml.Util;

namespace Yaml;

public class PrototypeYaml : SingletonDict<string, List<Prototype>>
{
	public static Prototype GetItemPrototype(string prototypeId, int level)
	{
		List<Prototype> list = SingletonDict<string, List<Prototype>>.Get(prototypeId);
		if (list == null)
		{
			return null;
		}
		for (int i = 0; i < list.Count; i++)
		{
			Prototype prototype = list[i];
			if (prototype.min_level <= level && level <= prototype.max_level)
			{
				return prototype;
			}
		}
		return null;
	}
}
