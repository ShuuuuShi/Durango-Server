using System.Collections.Generic;

namespace Shared.Market;

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
