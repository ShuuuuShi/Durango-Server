using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GradeComparer : IEqualityComparer<Grade>
{
	public bool Equals(Grade x, Grade y)
	{
		return x == y;
	}

	public int GetHashCode(Grade x)
	{
		return (int)x;
	}
}
