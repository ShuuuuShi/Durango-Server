using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Teleport;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TeleportTypeComparer : IEqualityComparer<TeleportType>
{
	public bool Equals(TeleportType x, TeleportType y)
	{
		return x == y;
	}

	public int GetHashCode(TeleportType x)
	{
		return (int)x;
	}
}
