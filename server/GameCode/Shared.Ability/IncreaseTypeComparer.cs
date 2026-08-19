using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Ability;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct IncreaseTypeComparer : IEqualityComparer<IncreaseType>
{
	public bool Equals(IncreaseType x, IncreaseType y)
	{
		return x == y;
	}

	public int GetHashCode(IncreaseType x)
	{
		return (int)x;
	}
}
