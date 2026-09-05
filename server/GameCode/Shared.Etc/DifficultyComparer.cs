using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Etc;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct DifficultyComparer : IEqualityComparer<Difficulty>
{
	public bool Equals(Difficulty x, Difficulty y)
	{
		return x == y;
	}

	public int GetHashCode(Difficulty x)
	{
		return (int)x;
	}
}
