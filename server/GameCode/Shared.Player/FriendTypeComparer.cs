using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Player;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FriendTypeComparer : IEqualityComparer<FriendType>
{
	public bool Equals(FriendType x, FriendType y)
	{
		return x == y;
	}

	public int GetHashCode(FriendType x)
	{
		return (int)x;
	}
}
