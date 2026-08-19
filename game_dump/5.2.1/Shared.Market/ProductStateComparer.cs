using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Market;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ProductStateComparer : IEqualityComparer<ProductState>
{
	public bool Equals(ProductState x, ProductState y)
	{
		return x == y;
	}

	public int GetHashCode(ProductState x)
	{
		return (int)x;
	}
}
