using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ResultComparer : IEqualityComparer<Result>
{
	public bool Equals(Result x, Result y)
	{
		return x == y;
	}

	public int GetHashCode(Result x)
	{
		return (int)x;
	}
}
