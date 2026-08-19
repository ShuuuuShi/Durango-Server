using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ActionSetTypeComparer : IEqualityComparer<ActionSetType>
{
	public bool Equals(ActionSetType x, ActionSetType y)
	{
		return x == y;
	}

	public int GetHashCode(ActionSetType x)
	{
		return (int)x;
	}
}
