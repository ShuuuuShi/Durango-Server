using System.Collections.Generic;

namespace Shared.Survival;

public struct FatigueCategoryComparer : IEqualityComparer<FatigueCategory>
{
	public bool Equals(FatigueCategory x, FatigueCategory y)
	{
		return x == y;
	}

	public int GetHashCode(FatigueCategory x)
	{
		return (int)x;
	}
}
