using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Market;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ProductSortFieldComparer : IEqualityComparer<ProductSortField>
{
	public bool Equals(ProductSortField x, ProductSortField y)
	{
		return x == y;
	}

	public int GetHashCode(ProductSortField x)
	{
		return (int)x;
	}
}
