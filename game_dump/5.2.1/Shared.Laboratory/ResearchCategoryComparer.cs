using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Laboratory;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ResearchCategoryComparer : IEqualityComparer<ResearchCategory>
{
	public bool Equals(ResearchCategory x, ResearchCategory y)
	{
		return x == y;
	}

	public int GetHashCode(ResearchCategory x)
	{
		return (int)x;
	}
}
