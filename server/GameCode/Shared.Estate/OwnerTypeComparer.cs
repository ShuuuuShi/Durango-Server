using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Estate;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct OwnerTypeComparer : IEqualityComparer<OwnerType>
{
	public bool Equals(OwnerType x, OwnerType y)
	{
		return x == y;
	}

	public int GetHashCode(OwnerType x)
	{
		return (int)x;
	}
}
