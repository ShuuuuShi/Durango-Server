using System.Collections.Generic;

namespace Shared.Etc;

public struct FitnessComparer : IEqualityComparer<Fitness>
{
	public bool Equals(Fitness x, Fitness y)
	{
		return x == y;
	}

	public int GetHashCode(Fitness x)
	{
		return (int)x;
	}
}
