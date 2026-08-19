using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Ability;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DerivedComparer : IEqualityComparer<Derived>
{
	public bool Equals(Derived x, Derived y)
	{
		return x == y;
	}

	public int GetHashCode(Derived x)
	{
		return (int)x;
	}
}
