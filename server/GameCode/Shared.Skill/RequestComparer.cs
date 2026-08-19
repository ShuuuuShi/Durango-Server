using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Skill;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
