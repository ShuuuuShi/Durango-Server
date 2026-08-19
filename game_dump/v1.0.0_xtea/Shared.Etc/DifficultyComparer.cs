using System.Collections.Generic;

namespace Shared.Etc;

public struct DifficultyComparer : IEqualityComparer<Difficulty>
{
	public bool Equals(Difficulty x, Difficulty y)
	{
		return x == y;
	}

	public int GetHashCode(Difficulty x)
	{
		return (int)x;
	}
}
