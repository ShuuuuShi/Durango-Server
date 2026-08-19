using System.Collections.Generic;

namespace Shared.System;

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
