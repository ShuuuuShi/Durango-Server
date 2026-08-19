using System.Collections.Generic;

namespace Shared.Player;

public struct JobComparer : IEqualityComparer<Job>
{
	public bool Equals(Job x, Job y)
	{
		return x == y;
	}

	public int GetHashCode(Job x)
	{
		return (int)x;
	}
}
