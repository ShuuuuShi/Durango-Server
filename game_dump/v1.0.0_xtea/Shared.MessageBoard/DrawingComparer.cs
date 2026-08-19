using System.Collections.Generic;

namespace Shared.MessageBoard;

public struct DrawingComparer : IEqualityComparer<Drawing>
{
	public bool Equals(Drawing x, Drawing y)
	{
		return x == y;
	}

	public int GetHashCode(Drawing x)
	{
		return (int)x;
	}
}
