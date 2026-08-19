using System.Collections.Generic;

namespace Shared.Ability;

public struct DerivedComparer : IEqualityComparer<Derived>
{
	public bool Equals(Derived x, Derived y)
	{
		return x == y;
	}

	public int GetHashCode(Derived x)
	{
		return (int)x;
	}
}
