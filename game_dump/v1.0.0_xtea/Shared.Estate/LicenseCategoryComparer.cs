using System.Collections.Generic;

namespace Shared.Estate;

public struct LicenseCategoryComparer : IEqualityComparer<LicenseCategory>
{
	public bool Equals(LicenseCategory x, LicenseCategory y)
	{
		return x == y;
	}

	public int GetHashCode(LicenseCategory x)
	{
		return (int)x;
	}
}
