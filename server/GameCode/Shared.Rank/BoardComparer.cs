using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Rank;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct BoardComparer : IEqualityComparer<Board>
{
	public bool Equals(Board x, Board y)
	{
		return x == y;
	}

	public int GetHashCode(Board x)
	{
		return (int)x;
	}
}
