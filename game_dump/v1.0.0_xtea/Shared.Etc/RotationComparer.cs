using System.Collections.Generic;

namespace Shared.Etc;

public struct RotationComparer : IEqualityComparer<Rotation>
{
	public bool Equals(Rotation x, Rotation y)
	{
		return x == y;
	}

	public int GetHashCode(Rotation x)
	{
		return (int)x;
	}
}
