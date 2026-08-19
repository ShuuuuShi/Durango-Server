using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Building;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
