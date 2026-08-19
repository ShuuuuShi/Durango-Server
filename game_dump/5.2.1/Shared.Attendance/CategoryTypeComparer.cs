using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Attendance;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CategoryTypeComparer : IEqualityComparer<CategoryType>
{
	public bool Equals(CategoryType x, CategoryType y)
	{
		return x == y;
	}

	public int GetHashCode(CategoryType x)
	{
		return (int)x;
	}
}
