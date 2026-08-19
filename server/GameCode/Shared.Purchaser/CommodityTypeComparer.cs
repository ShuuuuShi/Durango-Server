using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Purchaser;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CommodityTypeComparer : IEqualityComparer<CommodityType>
{
	public bool Equals(CommodityType x, CommodityType y)
	{
		return x == y;
	}

	public int GetHashCode(CommodityType x)
	{
		return (int)x;
	}
}
