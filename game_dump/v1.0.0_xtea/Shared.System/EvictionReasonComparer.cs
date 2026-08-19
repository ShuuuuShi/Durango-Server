using System.Collections.Generic;

namespace Shared.System;

public struct EvictionReasonComparer : IEqualityComparer<EvictionReason>
{
	public bool Equals(EvictionReason x, EvictionReason y)
	{
		return x == y;
	}

	public int GetHashCode(EvictionReason x)
	{
		return (int)x;
	}
}
