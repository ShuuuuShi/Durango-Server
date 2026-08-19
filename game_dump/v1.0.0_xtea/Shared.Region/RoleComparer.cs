using System.Collections.Generic;

namespace Shared.Region;

public struct RoleComparer : IEqualityComparer<Role>
{
	public bool Equals(Role x, Role y)
	{
		return x == y;
	}

	public int GetHashCode(Role x)
	{
		return (int)x;
	}
}
