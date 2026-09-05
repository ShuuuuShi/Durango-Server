using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Display;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AccessoryLimitComparer : IEqualityComparer<AccessoryLimit>
{
	public bool Equals(AccessoryLimit x, AccessoryLimit y)
	{
		return x == y;
	}

	public int GetHashCode(AccessoryLimit x)
	{
		return (int)x;
	}
}
