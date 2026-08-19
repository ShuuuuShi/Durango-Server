using System.Collections.Generic;

namespace Shared.Building;

public struct ExclusiveComparer : IEqualityComparer<Exclusive>
{
	public bool Equals(Exclusive x, Exclusive y)
	{
		return x == y;
	}

	public int GetHashCode(Exclusive x)
	{
		return (int)x;
	}
}
