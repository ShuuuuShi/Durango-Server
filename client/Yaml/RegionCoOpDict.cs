using System.Collections.Generic;
using JetBrains.Annotations;
using Yaml.Util;

namespace Yaml;

public class RegionCoOpDict : SingletonDict<string, Dictionary<string, RegionCoOp>>
{
	[CanBeNull]
	public static RegionCoOp GetRegionCoOp([CanBeNull] string regionTemplateId, [CanBeNull] string coOpId)
	{
		if (string.IsNullOrEmpty(coOpId) || string.IsNullOrEmpty(regionTemplateId))
		{
			return null;
		}
		Dictionary<string, RegionCoOp> dictionary = SingletonDict<string, Dictionary<string, RegionCoOp>>.Instance.Get(regionTemplateId);
		if (dictionary == null)
		{
			return null;
		}
		RegionCoOp regionCoOp = dictionary.Get(coOpId);
		if (regionCoOp == null)
		{
			return null;
		}
		return regionCoOp;
	}
}
