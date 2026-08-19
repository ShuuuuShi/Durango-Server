using System.Collections.Generic;

namespace Shared.Skill;

public struct RequestComparer : IEqualityComparer<Request>
{
	public bool Equals(Request x, Request y)
	{
		return x == y;
	}

	public int GetHashCode(Request x)
	{
		return (int)x;
	}
}
