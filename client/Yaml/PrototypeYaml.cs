using System.Collections.Generic;
using JetBrains.Annotations;
using Yaml.Util;

namespace Yaml;

public class PrototypeYaml : SingletonDict<string, List<Prototype>>
{
	[CanBeNull]
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
			if (prototype.MinLevel <= level && level <= prototype.MaxLevel)
			{
				return prototype;
			}
		}
		return null;
	}

	[CanBeNull]
	public static Prototype GetItemPrototype(string prototypeId)
	{
		List<Prototype> list = SingletonDict<string, List<Prototype>>.Get(prototypeId);
		if (list == null || list.Count == 0)
		{
			return null;
		}
		return list[0];
	}
}
