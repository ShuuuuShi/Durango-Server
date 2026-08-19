using System.Collections.Generic;
using JetBrains.Annotations;
using Newtonsoft.Json;
using Yaml.Util;

namespace Yaml;

public class Pioneer : Singleton<Pioneer>
{
	[JsonProperty(PropertyName = "daily_cost_exchange_rate")]
	public PioneerCostExchangeRate[] DailyCostExchangeRate;

	[JsonProperty(PropertyName = "grade_point")]
	public Dictionary<int, int> GradePoint;

	[JsonProperty(PropertyName = "region_access")]
	public int[][] RegionAccess;

	[CanBeNull]
	public PioneerCostExchangeRate GetPioneerCostExchangeRate(int grade)
	{
		PioneerCostExchangeRate[] dailyCostExchangeRate = DailyCostExchangeRate;
		foreach (PioneerCostExchangeRate pioneerCostExchangeRate in dailyCostExchangeRate)
		{
			if (pioneerCostExchangeRate.Grade == grade)
			{
				return pioneerCostExchangeRate;
			}
		}
		return null;
	}

	public int GetNextGradePoint(int grade)
	{
		return GradePoint.Get(grade + 1, 0);
	}

	public int GetAcceptableGrade(int unstableFactor)
	{
		if (unstableFactor < 1)
		{
			return 0;
		}
		for (int i = 0; i < RegionAccess.Length; i++)
		{
			if (RegionAccess[i][1] >= unstableFactor)
			{
				return RegionAccess[i][0];
			}
		}
		return RegionAccess[RegionAccess.Length - 1][0];
	}
}
