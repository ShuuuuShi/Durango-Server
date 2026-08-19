using System.Collections.Generic;

namespace Shared.Item;

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
