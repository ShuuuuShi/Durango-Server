using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
