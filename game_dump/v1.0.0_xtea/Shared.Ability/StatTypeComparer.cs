using System.Collections.Generic;

namespace Shared.Ability;

public struct StatTypeComparer : IEqualityComparer<StatType>
{
	public bool Equals(StatType x, StatType y)
	{
		return x == y;
	}

	public int GetHashCode(StatType x)
	{
		return (int)x;
	}
}
