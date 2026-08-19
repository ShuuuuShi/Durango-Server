using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Animal;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AnimalStatusComparer : IEqualityComparer<AnimalStatus>
{
	public bool Equals(AnimalStatus x, AnimalStatus y)
	{
		return x == y;
	}

	public int GetHashCode(AnimalStatus x)
	{
		return (int)x;
	}
}
