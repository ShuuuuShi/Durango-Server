using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
