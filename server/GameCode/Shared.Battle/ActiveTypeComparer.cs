using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ActiveTypeComparer : IEqualityComparer<ActiveType>
{
	public bool Equals(ActiveType x, ActiveType y)
	{
		return x == y;
	}

	public int GetHashCode(ActiveType x)
	{
		return (int)x;
	}
}
