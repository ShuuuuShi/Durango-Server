using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DamageTypeComparer : IEqualityComparer<DamageType>
{
	public bool Equals(DamageType x, DamageType y)
	{
		return x == y;
	}

	public int GetHashCode(DamageType x)
	{
		return (int)x;
	}
}
