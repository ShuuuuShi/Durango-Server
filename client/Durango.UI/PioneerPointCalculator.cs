using System.Collections.Generic;
using UnityEngine;
using Yaml;
using Yaml.Util;

namespace Durango.UI;

public static class PioneerPointCalculator
{
	public interface IPioneerProvider
	{
		PioneerCostExchangeRate GetPioneerCostExchangeRate(int grade);

		int GetNextGradePoint(int grade);
	}

	public class DefaultPioneerProvider : IPioneerProvider
	{
		public PioneerCostExchangeRate GetPioneerCostExchangeRate(int grade)
		{
			return Singleton<Pioneer>.Instance.GetPioneerCostExchangeRate(grade);
		}

		public int GetNextGradePoint(int grade)
		{
			return Singleton<Pioneer>.Instance.GetNextGradePoint(grade);
		}
	}

	private static readonly IPioneerProvider DefaultProvider = new DefaultPioneerProvider();

	public static float Run(Dictionary<float, float> exchangedPoints, ref int grade, ref float curGradePoint, bool paid, float itemPoint, IPioneerProvider provider = null)
	{
		if (provider == null)
		{
			provider = DefaultProvider;
		}
		float remainItemPoint = Mathf.Max(0f, itemPoint);
		float result;
		while (true)
		{
			result = 0f;
			PioneerCostExchangeRate pioneerCostExchangeRate = provider.GetPioneerCostExchangeRate(grade);
			if (pioneerCostExchangeRate == null)
			{
				break;
			}
			int nextGradePoint = provider.GetNextGradePoint(grade);
			if (nextGradePoint <= 0)
			{
				break;
			}
			PioneerRate[] rates = pioneerCostExchangeRate.Rates;
			foreach (PioneerRate pioneerRate in rates)
			{
				if (!pioneerRate.Paid || paid)
				{
					float num = exchangedPoints.Get(pioneerRate.Rate, 0f);
					num += CalcPoint(pioneerRate, num, ref remainItemPoint, ref curGradePoint, nextGradePoint);
					exchangedPoints[pioneerRate.Rate] = num;
					result = pioneerRate.Rate;
					if (num < (float)pioneerRate.Point)
					{
						break;
					}
				}
			}
			if (curGradePoint >= (float)nextGradePoint)
			{
				exchangedPoints.Clear();
				curGradePoint -= nextGradePoint;
				grade++;
				continue;
			}
			if (!(remainItemPoint > 0f))
			{
			}
			break;
		}
		return result;
	}

	private static float CalcPoint(PioneerRate rate, float exchangedPoint, ref float remainItemPoint, ref float curGradePoint, int nextGradePoint)
	{
		float remainPoint = rate.GetRemainPoint(exchangedPoint);
		float a = Mathf.Min(remainPoint, rate.Rate * remainItemPoint);
		float b = Mathf.Max(0f, (float)nextGradePoint - curGradePoint);
		a = Mathf.Min(a, b);
		float num = a / rate.Rate;
		if (Mathf.Approximately(remainItemPoint, num))
		{
			remainItemPoint = 0f;
		}
		else
		{
			remainItemPoint -= num;
		}
		curGradePoint += a;
		return a;
	}
}
