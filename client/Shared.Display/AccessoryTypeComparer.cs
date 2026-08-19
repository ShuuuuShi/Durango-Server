using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Display;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AccessoryTypeComparer : IEqualityComparer<AccessoryType>
{
	public bool Equals(AccessoryType x, AccessoryType y)
	{
		return x == y;
	}

	public int GetHashCode(AccessoryType x)
	{
		return (int)x;
	}
}
