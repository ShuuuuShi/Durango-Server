using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SourceDescriptionComparer : IEqualityComparer<SourceDescription>
{
	public bool Equals(SourceDescription x, SourceDescription y)
	{
		return x == y;
	}

	public int GetHashCode(SourceDescription x)
	{
		return (int)x;
	}
}
