using System.Collections.Generic;

namespace Shared.Estate;

public struct OwnerTypeComparer : IEqualityComparer<OwnerType>
{
	public bool Equals(OwnerType x, OwnerType y)
	{
		return x == y;
	}

	public int GetHashCode(OwnerType x)
	{
		return (int)x;
	}
}
