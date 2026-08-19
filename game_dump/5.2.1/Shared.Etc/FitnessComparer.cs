using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Etc;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct FitnessComparer : IEqualityComparer<Fitness>
{
	public bool Equals(Fitness x, Fitness y)
	{
		return x == y;
	}

	public int GetHashCode(Fitness x)
	{
		return (int)x;
	}
}
