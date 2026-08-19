using System.Collections.Generic;

namespace Shared.ClanFund;

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
