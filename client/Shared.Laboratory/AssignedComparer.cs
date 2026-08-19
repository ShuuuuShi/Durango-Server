using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Laboratory;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AssignedComparer : IEqualityComparer<Assigned>
{
	public bool Equals(Assigned x, Assigned y)
	{
		return x == y;
	}

	public int GetHashCode(Assigned x)
	{
		return (int)x;
	}
}
