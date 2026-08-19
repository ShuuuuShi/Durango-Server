using System.Collections.Generic;

namespace Shared.Battle;

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
