using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Region;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DateTimeComparer : IEqualityComparer<DateTime>
{
	public bool Equals(DateTime x, DateTime y)
	{
		return x == y;
	}

	public int GetHashCode(DateTime x)
	{
		return (int)x;
	}
}
