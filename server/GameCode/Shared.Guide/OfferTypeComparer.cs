using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Guide;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
