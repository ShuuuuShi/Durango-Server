using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Quest;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CoOpStateComparer : IEqualityComparer<CoOpState>
{
	public bool Equals(CoOpState x, CoOpState y)
	{
		return x == y;
	}

	public int GetHashCode(CoOpState x)
	{
		return (int)x;
	}
}
