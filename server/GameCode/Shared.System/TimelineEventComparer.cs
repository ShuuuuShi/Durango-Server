using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.System;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
