using System.Collections.Generic;

namespace Shared.Estate;

public struct AccessRightsComparer : IEqualityComparer<AccessRights>
{
	public bool Equals(AccessRights x, AccessRights y)
	{
		return x == y;
	}

	public int GetHashCode(AccessRights x)
	{
		return (int)x;
	}
}
