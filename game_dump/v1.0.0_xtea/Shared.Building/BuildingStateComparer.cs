using System.Collections.Generic;

namespace Shared.Building;

public struct BuildingStateComparer : IEqualityComparer<BuildingState>
{
	public bool Equals(BuildingState x, BuildingState y)
	{
		return x == y;
	}

	public int GetHashCode(BuildingState x)
	{
		return (int)x;
	}
}
