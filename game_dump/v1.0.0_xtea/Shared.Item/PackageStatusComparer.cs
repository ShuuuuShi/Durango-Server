using System.Collections.Generic;

namespace Shared.Item;

public struct PackageStatusComparer : IEqualityComparer<PackageStatus>
{
	public bool Equals(PackageStatus x, PackageStatus y)
	{
		return x == y;
	}

	public int GetHashCode(PackageStatus x)
	{
		return (int)x;
	}
}
