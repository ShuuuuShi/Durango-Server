using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ProhibitTypeComparer : IEqualityComparer<ProhibitType>
{
	public bool Equals(ProhibitType x, ProhibitType y)
	{
		return x == y;
	}

	public int GetHashCode(ProhibitType x)
	{
		return (int)x;
	}
}
