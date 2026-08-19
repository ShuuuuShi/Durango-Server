using System.Collections.Generic;

namespace Shared.System;

public struct InteractionComparer : IEqualityComparer<Interaction>
{
	public bool Equals(Interaction x, Interaction y)
	{
		return x == y;
	}

	public int GetHashCode(Interaction x)
	{
		return (int)x;
	}
}
