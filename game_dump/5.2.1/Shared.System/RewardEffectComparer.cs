using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.System;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RewardEffectComparer : IEqualityComparer<RewardEffect>
{
	public bool Equals(RewardEffect x, RewardEffect y)
	{
		return x == y;
	}

	public int GetHashCode(RewardEffect x)
	{
		return (int)x;
	}
}
