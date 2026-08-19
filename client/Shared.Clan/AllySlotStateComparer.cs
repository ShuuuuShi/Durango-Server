using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Clan;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AllySlotStateComparer : IEqualityComparer<AllySlotState>
{
	public bool Equals(AllySlotState x, AllySlotState y)
	{
		return x == y;
	}

	public int GetHashCode(AllySlotState x)
	{
		return (int)x;
	}
}
