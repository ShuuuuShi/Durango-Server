using System.Collections.Generic;

namespace Shared.Item;

public struct CraftTypeComparer : IEqualityComparer<CraftType>
{
	public bool Equals(CraftType x, CraftType y)
	{
		return x == y;
	}

	public int GetHashCode(CraftType x)
	{
		return (int)x;
	}
}
