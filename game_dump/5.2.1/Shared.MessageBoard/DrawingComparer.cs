using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.MessageBoard;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DrawingComparer : IEqualityComparer<Drawing>
{
	public bool Equals(Drawing x, Drawing y)
	{
		return x == y;
	}

	public int GetHashCode(Drawing x)
	{
		return (int)x;
	}
}
