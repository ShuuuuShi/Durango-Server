using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Clan;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
