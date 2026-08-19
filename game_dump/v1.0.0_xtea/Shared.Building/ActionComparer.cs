using System.Collections.Generic;

namespace Shared.Building;

public struct ActionComparer : IEqualityComparer<Action>
{
	public bool Equals(Action x, Action y)
	{
		return x == y;
	}

	public int GetHashCode(Action x)
	{
		return (int)x;
	}
}
