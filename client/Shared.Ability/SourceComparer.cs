using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Ability;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SourceComparer : IEqualityComparer<Source>
{
	public bool Equals(Source x, Source y)
	{
		return x == y;
	}

	public int GetHashCode(Source x)
	{
		return (int)x;
	}
}
