using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Building;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ActionComparer : IEqualityComparer<Action>
{
	public bool Equals(Action x, Action y)
	{
		return x == y;
	}

	public int GetHashCode(Action x)
	{
		return (int)x;
	}
}
