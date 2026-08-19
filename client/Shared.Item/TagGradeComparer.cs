using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct TagGradeComparer : IEqualityComparer<TagGrade>
{
	public bool Equals(TagGrade x, TagGrade y)
	{
		return x == y;
	}

	public int GetHashCode(TagGrade x)
	{
		return (int)x;
	}
}
