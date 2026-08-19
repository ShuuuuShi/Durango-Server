using System.Collections.Generic;

namespace Shared.Rank;

public struct CategoryComparer : IEqualityComparer<Category>
{
	public bool Equals(Category x, Category y)
	{
		return x == y;
	}

	public int GetHashCode(Category x)
	{
		return (int)x;
	}
}
