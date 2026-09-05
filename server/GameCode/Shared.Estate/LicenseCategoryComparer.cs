using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Estate;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
