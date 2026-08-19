using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Purchaser;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TagsComparer : IEqualityComparer<Tags>
{
	public bool Equals(Tags x, Tags y)
	{
		return x == y;
	}

	public int GetHashCode(Tags x)
	{
		return (int)x;
	}
}
