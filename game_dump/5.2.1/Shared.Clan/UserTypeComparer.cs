using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Clan;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct UserTypeComparer : IEqualityComparer<UserType>
{
	public bool Equals(UserType x, UserType y)
	{
		return x == y;
	}

	public int GetHashCode(UserType x)
	{
		return (int)x;
	}
}
