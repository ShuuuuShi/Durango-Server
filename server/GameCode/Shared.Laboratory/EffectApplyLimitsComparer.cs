using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Laboratory;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
