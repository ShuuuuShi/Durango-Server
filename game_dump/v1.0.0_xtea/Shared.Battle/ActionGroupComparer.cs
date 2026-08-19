using System.Collections.Generic;

namespace Shared.Battle;

public struct ActionGroupComparer : IEqualityComparer<ActionGroup>
{
	public bool Equals(ActionGroup x, ActionGroup y)
	{
		return x == y;
	}

	public int GetHashCode(ActionGroup x)
	{
		return (int)x;
	}
}
