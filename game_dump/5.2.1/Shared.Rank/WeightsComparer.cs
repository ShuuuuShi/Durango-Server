using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Rank;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct WeightsComparer : IEqualityComparer<Weights>
{
	public bool Equals(Weights x, Weights y)
	{
		return x == y;
	}

	public int GetHashCode(Weights x)
	{
		return (int)x;
	}
}
