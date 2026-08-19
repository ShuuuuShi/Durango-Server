using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Animal;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct PetTaskTypeComparer : IEqualityComparer<PetTaskType>
{
	public bool Equals(PetTaskType x, PetTaskType y)
	{
		return x == y;
	}

	public int GetHashCode(PetTaskType x)
	{
		return (int)x;
	}
}
