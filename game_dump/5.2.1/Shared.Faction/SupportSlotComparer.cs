using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Faction;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SupportSlotComparer : IEqualityComparer<SupportSlot>
{
	public bool Equals(SupportSlot x, SupportSlot y)
	{
		return x == y;
	}

	public int GetHashCode(SupportSlot x)
	{
		return (int)x;
	}
}
