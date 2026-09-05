using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Attendance;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RewardTypeComparer : IEqualityComparer<RewardType>
{
	public bool Equals(RewardType x, RewardType y)
	{
		return x == y;
	}

	public int GetHashCode(RewardType x)
	{
		return (int)x;
	}
}
