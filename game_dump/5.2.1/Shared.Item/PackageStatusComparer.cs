using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
