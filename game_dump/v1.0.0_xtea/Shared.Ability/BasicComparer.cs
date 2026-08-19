using System.Collections.Generic;

namespace Shared.Ability;

public struct BasicComparer : IEqualityComparer<Basic>
{
	public bool Equals(Basic x, Basic y)
	{
		return x == y;
	}

	public int GetHashCode(Basic x)
	{
		return (int)x;
	}
}
