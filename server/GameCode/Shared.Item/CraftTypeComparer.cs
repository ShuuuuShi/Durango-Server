using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CraftTypeComparer : IEqualityComparer<CraftType>
{
	public bool Equals(CraftType x, CraftType y)
	{
		return x == y;
	}

	public int GetHashCode(CraftType x)
	{
		return (int)x;
	}
}
