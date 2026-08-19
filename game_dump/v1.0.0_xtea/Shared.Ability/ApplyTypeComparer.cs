using System.Collections.Generic;

namespace Shared.Ability;

public struct ApplyTypeComparer : IEqualityComparer<ApplyType>
{
	public bool Equals(ApplyType x, ApplyType y)
	{
		return x == y;
	}

	public int GetHashCode(ApplyType x)
	{
		return (int)x;
	}
}
