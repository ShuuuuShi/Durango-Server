using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Ability;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RepresentTypeComparer : IEqualityComparer<RepresentType>
{
	public bool Equals(RepresentType x, RepresentType y)
	{
		return x == y;
	}

	public int GetHashCode(RepresentType x)
	{
		return (int)x;
	}
}
