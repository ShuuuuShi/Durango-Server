using System.Collections.Generic;

namespace Shared.Rank;

public struct AggregatedCategoryComparer : IEqualityComparer<AggregatedCategory>
{
	public bool Equals(AggregatedCategory x, AggregatedCategory y)
	{
		return x == y;
	}

	public int GetHashCode(AggregatedCategory x)
	{
		return (int)x;
	}
}
