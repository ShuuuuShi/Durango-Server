using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Region;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
