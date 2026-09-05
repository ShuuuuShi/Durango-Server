using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Encyclopedia;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct EncyclopediaTypeComparer : IEqualityComparer<EncyclopediaType>
{
	public bool Equals(EncyclopediaType x, EncyclopediaType y)
	{
		return x == y;
	}

	public int GetHashCode(EncyclopediaType x)
	{
		return (int)x;
	}
}
