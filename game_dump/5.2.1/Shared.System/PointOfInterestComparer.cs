using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.System;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PointOfInterestComparer : IEqualityComparer<PointOfInterest>
{
	public bool Equals(PointOfInterest x, PointOfInterest y)
	{
		return x == y;
	}

	public int GetHashCode(PointOfInterest x)
	{
		return (int)x;
	}
}
