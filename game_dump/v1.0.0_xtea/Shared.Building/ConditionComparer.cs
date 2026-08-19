using System.Collections.Generic;

namespace Shared.Building;

public struct ConditionComparer : IEqualityComparer<Condition>
{
	public bool Equals(Condition x, Condition y)
	{
		return x == y;
	}

	public int GetHashCode(Condition x)
	{
		return (int)x;
	}
}
