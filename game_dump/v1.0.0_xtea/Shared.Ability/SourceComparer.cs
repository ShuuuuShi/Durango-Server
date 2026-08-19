using System.Collections.Generic;

namespace Shared.Ability;

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
