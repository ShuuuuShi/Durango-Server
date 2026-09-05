using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Purchaser;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct EmotionTierComparer : IEqualityComparer<EmotionTier>
{
	public bool Equals(EmotionTier x, EmotionTier y)
	{
		return x == y;
	}

	public int GetHashCode(EmotionTier x)
	{
		return (int)x;
	}
}
