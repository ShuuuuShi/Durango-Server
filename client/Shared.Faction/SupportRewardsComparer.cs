using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Faction;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SupportRewardsComparer : IEqualityComparer<SupportRewards>
{
	public bool Equals(SupportRewards x, SupportRewards y)
	{
		return x == y;
	}

	public int GetHashCode(SupportRewards x)
	{
		return (int)x;
	}
}
