using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct InterruptTypeComparer : IEqualityComparer<InterruptType>
{
	public bool Equals(InterruptType x, InterruptType y)
	{
		return x == y;
	}

	public int GetHashCode(InterruptType x)
	{
		return (int)x;
	}
}
