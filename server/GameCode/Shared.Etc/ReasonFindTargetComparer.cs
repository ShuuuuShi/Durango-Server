using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Etc;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ReasonFindTargetComparer : IEqualityComparer<ReasonFindTarget>
{
	public bool Equals(ReasonFindTarget x, ReasonFindTarget y)
	{
		return x == y;
	}

	public int GetHashCode(ReasonFindTarget x)
	{
		return (int)x;
	}
}
