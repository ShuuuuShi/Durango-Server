using System.Collections.Generic;

namespace Shared.Laboratory;

public struct ResearchCategoryComparer : IEqualityComparer<ResearchCategory>
{
	public bool Equals(ResearchCategory x, ResearchCategory y)
	{
		return x == y;
	}

	public int GetHashCode(ResearchCategory x)
	{
		return (int)x;
	}
}
