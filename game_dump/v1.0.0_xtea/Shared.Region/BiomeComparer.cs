using System.Collections.Generic;

namespace Shared.Region;

public struct BiomeComparer : IEqualityComparer<Biome>
{
	public bool Equals(Biome x, Biome y)
	{
		return x == y;
	}

	public int GetHashCode(Biome x)
	{
		return (int)x;
	}
}
