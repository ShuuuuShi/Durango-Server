using System.Collections.Generic;

namespace Shared.Etc;

public struct DirectionComparer : IEqualityComparer<Direction>
{
	public bool Equals(Direction x, Direction y)
	{
		return x == y;
	}

	public int GetHashCode(Direction x)
	{
		return (int)x;
	}
}
