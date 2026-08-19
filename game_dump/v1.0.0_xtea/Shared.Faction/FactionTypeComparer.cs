using System.Collections.Generic;

namespace Shared.Faction;

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
