using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Survival;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DirectionComparer : IEqualityComparer<Direction>
{
	public bool Equals(Direction x, Direction y)
	{
		return x == y;
	}

	public int GetHashCode(Direction x)
	{
		return (int)x;
	}
}
