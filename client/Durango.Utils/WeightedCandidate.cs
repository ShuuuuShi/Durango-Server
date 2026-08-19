using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Utils;

public class WeightedCandidate
{
	public float Weight;

	[CanBeNull]
	public static T Select<T>(IList<T> candidates) where T : WeightedCandidate
	{
		int count = candidates.Count;
		float num = 0f;
		for (int i = 0; i < count; i++)
		{
			num += candidates[i].Weight;
		}
		float num2 = Random.value * num;
		for (int j = 0; j < count; j++)
		{
			num2 -= candidates[j].Weight;
			if (num2 <= 0f)
			{
				return candidates[j];
			}
		}
		return (T)null;
	}
}
