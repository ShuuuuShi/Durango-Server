using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Economy;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
