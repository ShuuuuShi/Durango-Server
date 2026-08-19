using System.Collections.Generic;

namespace Shared.Animal;

public struct AnimalStatusComparer : IEqualityComparer<AnimalStatus>
{
	public bool Equals(AnimalStatus x, AnimalStatus y)
	{
		return x == y;
	}

	public int GetHashCode(AnimalStatus x)
	{
		return (int)x;
	}
}
