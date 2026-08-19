using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.ClanFund;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FundTypeComparer : IEqualityComparer<FundType>
{
	public bool Equals(FundType x, FundType y)
	{
		return x == y;
	}

	public int GetHashCode(FundType x)
	{
		return (int)x;
	}
}
