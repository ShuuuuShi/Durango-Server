using System.Collections.Generic;

namespace Shared.Survival;

public struct ResetStrategyComparer : IEqualityComparer<ResetStrategy>
{
	public bool Equals(ResetStrategy x, ResetStrategy y)
	{
		return x == y;
	}

	public int GetHashCode(ResetStrategy x)
	{
		return (int)x;
	}
}
