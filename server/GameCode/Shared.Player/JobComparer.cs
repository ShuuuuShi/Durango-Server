using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Player;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct JobComparer : IEqualityComparer<Job>
{
	public bool Equals(Job x, Job y)
	{
		return x == y;
	}

	public int GetHashCode(Job x)
	{
		return (int)x;
	}
}
