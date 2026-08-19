using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Rank;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
