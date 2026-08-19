using System.Collections.Generic;

namespace ChatData;

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
