using System.Collections.Generic;

namespace Shared.Guide;

public struct OfferTypeComparer : IEqualityComparer<OfferType>
{
	public bool Equals(OfferType x, OfferType y)
	{
		return x == y;
	}

	public int GetHashCode(OfferType x)
	{
		return (int)x;
	}
}
