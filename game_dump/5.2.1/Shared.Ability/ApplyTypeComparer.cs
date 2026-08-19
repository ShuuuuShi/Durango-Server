using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Ability;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ApplyTypeComparer : IEqualityComparer<ApplyType>
{
	public bool Equals(ApplyType x, ApplyType y)
	{
		return x == y;
	}

	public int GetHashCode(ApplyType x)
	{
		return (int)x;
	}
}
