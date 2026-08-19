using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Building;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ExclusiveComparer : IEqualityComparer<Exclusive>
{
	public bool Equals(Exclusive x, Exclusive y)
	{
		return x == y;
	}

	public int GetHashCode(Exclusive x)
	{
		return (int)x;
	}
}
