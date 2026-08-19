using System.Collections.Generic;

namespace Shared.Skill;

public struct RewardTypeComparer : IEqualityComparer<RewardType>
{
	public bool Equals(RewardType x, RewardType y)
	{
		return x == y;
	}

	public int GetHashCode(RewardType x)
	{
		return (int)x;
	}
}
