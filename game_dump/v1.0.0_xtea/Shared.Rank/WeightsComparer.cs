using System.Collections.Generic;

namespace Shared.Rank;

public struct WeightsComparer : IEqualityComparer<Weights>
{
	public bool Equals(Weights x, Weights y)
	{
		return x == y;
	}

	public int GetHashCode(Weights x)
	{
		return (int)x;
	}
}
