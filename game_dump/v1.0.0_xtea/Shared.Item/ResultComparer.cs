using System.Collections.Generic;

namespace Shared.Item;

public struct ResultComparer : IEqualityComparer<Result>
{
	public bool Equals(Result x, Result y)
	{
		return x == y;
	}

	public int GetHashCode(Result x)
	{
		return (int)x;
	}
}
