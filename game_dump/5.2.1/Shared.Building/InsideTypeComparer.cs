using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Building;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct InsideTypeComparer : IEqualityComparer<InsideType>
{
	public bool Equals(InsideType x, InsideType y)
	{
		return x == y;
	}

	public int GetHashCode(InsideType x)
	{
		return (int)x;
	}
}
