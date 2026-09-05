using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
