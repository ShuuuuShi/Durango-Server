using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Display;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct BoardingOnComparer : IEqualityComparer<BoardingOn>
{
	public bool Equals(BoardingOn x, BoardingOn y)
	{
		return x == y;
	}

	public int GetHashCode(BoardingOn x)
	{
		return (int)x;
	}
}
