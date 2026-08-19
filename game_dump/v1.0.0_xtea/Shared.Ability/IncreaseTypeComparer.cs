using System.Collections.Generic;

namespace Shared.Ability;

public struct IncreaseTypeComparer : IEqualityComparer<IncreaseType>
{
	public bool Equals(IncreaseType x, IncreaseType y)
	{
		return x == y;
	}

	public int GetHashCode(IncreaseType x)
	{
		return (int)x;
	}
}
