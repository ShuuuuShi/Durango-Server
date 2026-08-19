using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Season2;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ResourceTypeComparer : IEqualityComparer<ResourceType>
{
	public bool Equals(ResourceType x, ResourceType y)
	{
		return x == y;
	}

	public int GetHashCode(ResourceType x)
	{
		return (int)x;
	}
}
