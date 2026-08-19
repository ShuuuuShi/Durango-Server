using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Survival;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FatigueCategoryComparer : IEqualityComparer<FatigueCategory>
{
	public bool Equals(FatigueCategory x, FatigueCategory y)
	{
		return x == y;
	}

	public int GetHashCode(FatigueCategory x)
	{
		return (int)x;
	}
}
