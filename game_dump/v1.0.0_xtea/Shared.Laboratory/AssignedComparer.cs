using System.Collections.Generic;

namespace Shared.Laboratory;

public struct AssignedComparer : IEqualityComparer<Assigned>
{
	public bool Equals(Assigned x, Assigned y)
	{
		return x == y;
	}

	public int GetHashCode(Assigned x)
	{
		return (int)x;
	}
}
