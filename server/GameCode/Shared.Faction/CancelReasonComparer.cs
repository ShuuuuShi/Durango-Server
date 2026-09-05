using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Faction;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
