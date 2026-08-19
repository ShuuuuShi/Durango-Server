using System.Collections.Generic;

namespace Shared.Inspect;

public struct NaturalHealthStatusComparer : IEqualityComparer<NaturalHealthStatus>
{
	public bool Equals(NaturalHealthStatus x, NaturalHealthStatus y)
	{
		return x == y;
	}

	public int GetHashCode(NaturalHealthStatus x)
	{
		return (int)x;
	}
}
