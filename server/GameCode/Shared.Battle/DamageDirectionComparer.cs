using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
