using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Laboratory;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct LaboratoryTierComparer : IEqualityComparer<LaboratoryTier>
{
	public bool Equals(LaboratoryTier x, LaboratoryTier y)
	{
		return x == y;
	}

	public int GetHashCode(LaboratoryTier x)
	{
		return (int)x;
	}
}
