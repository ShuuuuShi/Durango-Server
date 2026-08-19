using System.Collections.Generic;

namespace Shared.StatusEffect;

public struct EffectTypeComparer : IEqualityComparer<EffectType>
{
	public bool Equals(EffectType x, EffectType y)
	{
		return x == y;
	}

	public int GetHashCode(EffectType x)
	{
		return (int)x;
	}
}
