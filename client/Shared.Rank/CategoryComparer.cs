using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Rank;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CategoryComparer : IEqualityComparer<Category>
{
	public bool Equals(Category x, Category y)
	{
		return x == y;
	}

	public int GetHashCode(Category x)
	{
		return (int)x;
	}
}
