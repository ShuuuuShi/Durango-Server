using System.Collections.Generic;

namespace Shared.Battle;

public struct DamageResultComparer : IEqualityComparer<DamageResult>
{
	public bool Equals(DamageResult x, DamageResult y)
	{
		return x == y;
	}

	public int GetHashCode(DamageResult x)
	{
		return (int)x;
	}
}
