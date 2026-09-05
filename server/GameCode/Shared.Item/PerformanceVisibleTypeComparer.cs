using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PerformanceVisibleTypeComparer : IEqualityComparer<PerformanceVisibleType>
{
	public bool Equals(PerformanceVisibleType x, PerformanceVisibleType y)
	{
		return x == y;
	}

	public int GetHashCode(PerformanceVisibleType x)
	{
		return (int)x;
	}
}
