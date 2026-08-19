using System.Collections.Generic;

namespace Shared.Inspect;

public struct AnimalHealthStatusComparer : IEqualityComparer<AnimalHealthStatus>
{
	public bool Equals(AnimalHealthStatus x, AnimalHealthStatus y)
	{
		return x == y;
	}

	public int GetHashCode(AnimalHealthStatus x)
	{
		return (int)x;
	}
}
