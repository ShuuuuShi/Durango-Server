using System.Collections.Generic;

namespace Shared.Economy;

public struct CurrencyComparer : IEqualityComparer<Currency>
{
	public bool Equals(Currency x, Currency y)
	{
		return x == y;
	}

	public int GetHashCode(Currency x)
	{
		return (int)x;
	}
}
