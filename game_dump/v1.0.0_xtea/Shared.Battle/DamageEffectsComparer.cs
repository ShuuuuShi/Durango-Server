using System.Collections.Generic;

namespace Shared.Battle;

public struct DamageEffectsComparer : IEqualityComparer<DamageEffects>
{
	public bool Equals(DamageEffects x, DamageEffects y)
	{
		return x == y;
	}

	public int GetHashCode(DamageEffects x)
	{
		return (int)x;
	}
}
