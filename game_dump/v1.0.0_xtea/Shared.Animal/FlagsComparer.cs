using System.Collections.Generic;

namespace Shared.Animal;

public struct FlagsComparer : IEqualityComparer<Flags>
{
	public bool Equals(Flags x, Flags y)
	{
		return x == y;
	}

	public int GetHashCode(Flags x)
	{
		return (int)x;
	}
}
