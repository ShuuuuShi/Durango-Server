using System.Collections.Generic;
using Shared.Region;
using Yaml.Util;

namespace Yaml;

public class RegionTemplateDict : SingletonDict<string, RegionTemplate>
{
	public static string[] FindTemplateIdsByRole(Role role)
	{
		List<string> list = new List<string>();
		Enumerator enumerator = SingletonDict<string, RegionTemplate>.Instance.GetEnumerator();
		while (enumerator.MoveNext())
		{
			if (enumerator.Current.Value.role == role)
			{
				list.Add(enumerator.Current.Key);
			}
		}
		return list.ToArray();
	}
}
