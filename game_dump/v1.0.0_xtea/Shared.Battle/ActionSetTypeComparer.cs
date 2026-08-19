using System.Collections.Generic;

namespace Shared.Battle;

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
