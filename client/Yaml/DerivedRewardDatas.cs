using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Shared.Ability;
using Yaml.Util;

namespace Yaml;

public class DerivedRewardDatas : SingletonDict<Derived, DerivedRewardData[]>
{
	public static void Set([NotNull] Dictionary<Derived, Dictionary<int, DerivedRewardData>> rawData)
	{
		DerivedRewardDatas derivedRewardDatas = new DerivedRewardDatas();
		foreach (KeyValuePair<Derived, Dictionary<int, DerivedRewardData>> rawDatum in rawData)
		{
			DerivedRewardData[] array = rawDatum.Value.Values.ToArray();
			Array.Sort(array, (DerivedRewardData r1, DerivedRewardData r2) => r1.RequiredValue - r2.RequiredValue);
			derivedRewardDatas.Add(rawDatum.Key, array);
		}
		derivedRewardDatas.Initialize(derivedRewardDatas);
	}
}
