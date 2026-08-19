using System.Collections.Generic;

namespace Shared.Teleport;

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
