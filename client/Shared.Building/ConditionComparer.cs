using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Building;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ConditionComparer : IEqualityComparer<Condition>
{
	public bool Equals(Condition x, Condition y)
	{
		return x == y;
	}

	public int GetHashCode(Condition x)
	{
		return (int)x;
	}
}
