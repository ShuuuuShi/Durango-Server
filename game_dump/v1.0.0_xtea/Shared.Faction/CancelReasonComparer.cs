using System.Collections.Generic;

namespace Shared.Faction;

public struct CancelReasonComparer : IEqualityComparer<CancelReason>
{
	public bool Equals(CancelReason x, CancelReason y)
	{
		return x == y;
	}

	public int GetHashCode(CancelReason x)
	{
		return (int)x;
	}
}
