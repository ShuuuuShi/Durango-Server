using System.Collections.Generic;

namespace Shared.System;

public struct RewardEffectComparer : IEqualityComparer<RewardEffect>
{
	public bool Equals(RewardEffect x, RewardEffect y)
	{
		return x == y;
	}

	public int GetHashCode(RewardEffect x)
	{
		return (int)x;
	}
}
