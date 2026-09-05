using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Animal;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FlagsComparer : IEqualityComparer<Flags>
{
	public bool Equals(Flags x, Flags y)
	{
		return x == y;
	}

	public int GetHashCode(Flags x)
	{
		return (int)x;
	}
}
