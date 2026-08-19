using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Region;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
