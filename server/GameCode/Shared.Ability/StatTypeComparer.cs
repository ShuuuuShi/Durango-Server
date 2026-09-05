using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Ability;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct StatTypeComparer : IEqualityComparer<StatType>
{
	public bool Equals(StatType x, StatType y)
	{
		return x == y;
	}

	public int GetHashCode(StatType x)
	{
		return (int)x;
	}
}
