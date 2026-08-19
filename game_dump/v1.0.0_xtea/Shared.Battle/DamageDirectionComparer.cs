using System.Collections.Generic;

namespace Shared.Battle;

public struct DamageDirectionComparer : IEqualityComparer<DamageDirection>
{
	public bool Equals(DamageDirection x, DamageDirection y)
	{
		return x == y;
	}

	public int GetHashCode(DamageDirection x)
	{
		return (int)x;
	}
}
