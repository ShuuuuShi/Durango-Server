using System.Collections.Generic;

namespace Shared.Inspect;

public struct FailedInspectReasonComparer : IEqualityComparer<FailedInspectReason>
{
	public bool Equals(FailedInspectReason x, FailedInspectReason y)
	{
		return x == y;
	}

	public int GetHashCode(FailedInspectReason x)
	{
		return (int)x;
	}
}
