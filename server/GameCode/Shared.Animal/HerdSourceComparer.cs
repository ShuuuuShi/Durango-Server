using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Animal;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct HerdSourceComparer : IEqualityComparer<HerdSource>
{
	public bool Equals(HerdSource x, HerdSource y)
	{
		return x == y;
	}

	public int GetHashCode(HerdSource x)
	{
		return (int)x;
	}
}
