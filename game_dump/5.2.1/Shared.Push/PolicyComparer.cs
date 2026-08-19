using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Push;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PolicyComparer : IEqualityComparer<Policy>
{
	public bool Equals(Policy x, Policy y)
	{
		return x == y;
	}

	public int GetHashCode(Policy x)
	{
		return (int)x;
	}
}
