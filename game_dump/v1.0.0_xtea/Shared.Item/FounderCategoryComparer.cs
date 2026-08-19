using System.Collections.Generic;

namespace Shared.Item;

public struct FounderCategoryComparer : IEqualityComparer<FounderCategory>
{
	public bool Equals(FounderCategory x, FounderCategory y)
	{
		return x == y;
	}

	public int GetHashCode(FounderCategory x)
	{
		return (int)x;
	}
}
