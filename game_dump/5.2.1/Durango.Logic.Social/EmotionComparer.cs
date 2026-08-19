using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Durango.Logic.Social;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct EmotionComparer : IEqualityComparer<Emotion>
{
	public bool Equals(Emotion x, Emotion y)
	{
		return x == y;
	}

	public int GetHashCode(Emotion x)
	{
		return (int)x;
	}
}
