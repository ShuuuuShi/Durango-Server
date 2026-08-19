using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Explorer;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RegionAdaptationComparer : IEqualityComparer<RegionAdaptation>
{
	public bool Equals(RegionAdaptation x, RegionAdaptation y)
	{
		return x == y;
	}

	public int GetHashCode(RegionAdaptation x)
	{
		return (int)x;
	}
}
