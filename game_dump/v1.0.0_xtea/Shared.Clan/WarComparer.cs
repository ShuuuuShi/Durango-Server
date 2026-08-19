using System.Collections.Generic;

namespace Shared.Clan;

public struct WarComparer : IEqualityComparer<War>
{
	public bool Equals(War x, War y)
	{
		return x == y;
	}

	public int GetHashCode(War x)
	{
		return (int)x;
	}
}
