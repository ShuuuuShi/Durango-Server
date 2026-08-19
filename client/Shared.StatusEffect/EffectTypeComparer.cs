using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.StatusEffect;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
