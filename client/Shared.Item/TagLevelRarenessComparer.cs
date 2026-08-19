using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TagLevelRarenessComparer : IEqualityComparer<TagLevelRareness>
{
	public bool Equals(TagLevelRareness x, TagLevelRareness y)
	{
		return x == y;
	}

	public int GetHashCode(TagLevelRareness x)
	{
		return (int)x;
	}
}
