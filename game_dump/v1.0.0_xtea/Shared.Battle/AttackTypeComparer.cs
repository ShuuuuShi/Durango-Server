using System.Collections.Generic;

namespace Shared.Battle;

public struct AttackTypeComparer : IEqualityComparer<AttackType>
{
	public bool Equals(AttackType x, AttackType y)
	{
		return x == y;
	}

	public int GetHashCode(AttackType x)
	{
		return (int)x;
	}
}
