using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Faction;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FactionTypeComparer : IEqualityComparer<FactionType>
{
	public bool Equals(FactionType x, FactionType y)
	{
		return x == y;
	}

	public int GetHashCode(FactionType x)
	{
		return (int)x;
	}
}
