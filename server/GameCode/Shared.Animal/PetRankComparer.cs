using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Animal;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PetRankComparer : IEqualityComparer<PetRank>
{
	public bool Equals(PetRank x, PetRank y)
	{
		return x == y;
	}

	public int GetHashCode(PetRank x)
	{
		return (int)x;
	}
}
