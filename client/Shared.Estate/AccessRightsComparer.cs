using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Estate;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
