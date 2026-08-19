using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.System;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
