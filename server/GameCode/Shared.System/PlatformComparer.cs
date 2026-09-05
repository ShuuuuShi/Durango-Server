using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.System;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PlatformComparer : IEqualityComparer<Platform>
{
	public bool Equals(Platform x, Platform y)
	{
		return x == y;
	}

	public int GetHashCode(Platform x)
	{
		return (int)x;
	}
}
