using System.Collections.Generic;

namespace Shared.Item;

public struct PerformanceVisibleTypeComparer : IEqualityComparer<PerformanceVisibleType>
{
	public bool Equals(PerformanceVisibleType x, PerformanceVisibleType y)
	{
		return x == y;
	}

	public int GetHashCode(PerformanceVisibleType x)
	{
		return (int)x;
	}
}
