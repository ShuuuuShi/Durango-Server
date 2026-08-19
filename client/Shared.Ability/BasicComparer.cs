using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Ability;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct BasicComparer : IEqualityComparer<Basic>
{
	public bool Equals(Basic x, Basic y)
	{
		return x == y;
	}

	public int GetHashCode(Basic x)
	{
		return (int)x;
	}
}
