using System.Collections.Generic;

namespace Shared.Laboratory;

public struct EffectApplyLimitsComparer : IEqualityComparer<EffectApplyLimits>
{
	public bool Equals(EffectApplyLimits x, EffectApplyLimits y)
	{
		return x == y;
	}

	public int GetHashCode(EffectApplyLimits x)
	{
		return (int)x;
	}
}
