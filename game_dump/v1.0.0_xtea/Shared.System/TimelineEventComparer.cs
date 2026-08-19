using System.Collections.Generic;

namespace Shared.System;

public struct TimelineEventComparer : IEqualityComparer<TimelineEvent>
{
	public bool Equals(TimelineEvent x, TimelineEvent y)
	{
		return x == y;
	}

	public int GetHashCode(TimelineEvent x)
	{
		return (int)x;
	}
}
