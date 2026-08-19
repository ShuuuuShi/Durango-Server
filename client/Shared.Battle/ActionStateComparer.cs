using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ActionStateComparer : IEqualityComparer<ActionState>
{
	public bool Equals(ActionState x, ActionState y)
	{
		return x == y;
	}

	public int GetHashCode(ActionState x)
	{
		return (int)x;
	}
}
