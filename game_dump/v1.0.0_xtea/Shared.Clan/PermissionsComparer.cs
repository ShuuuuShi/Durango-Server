using System.Collections.Generic;

namespace Shared.Clan;

public struct PermissionsComparer : IEqualityComparer<Permissions>
{
	public bool Equals(Permissions x, Permissions y)
	{
		return x == y;
	}

	public int GetHashCode(Permissions x)
	{
		return (int)x;
	}
}
