using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Rank;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RivalComparer : IEqualityComparer<Rival>
{
	public bool Equals(Rival x, Rival y)
	{
		return x == y;
	}

	public int GetHashCode(Rival x)
	{
		return (int)x;
	}
}
