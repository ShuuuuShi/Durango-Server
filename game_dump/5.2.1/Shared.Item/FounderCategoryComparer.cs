using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FounderCategoryComparer : IEqualityComparer<FounderCategory>
{
	public bool Equals(FounderCategory x, FounderCategory y)
	{
		return x == y;
	}

	public int GetHashCode(FounderCategory x)
	{
		return (int)x;
	}
}
