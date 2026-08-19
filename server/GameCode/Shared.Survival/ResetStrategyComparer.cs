using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Survival;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ResetStrategyComparer : IEqualityComparer<ResetStrategy>
{
	public bool Equals(ResetStrategy x, ResetStrategy y)
	{
		return x == y;
	}

	public int GetHashCode(ResetStrategy x)
	{
		return (int)x;
	}
}
