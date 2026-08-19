using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Quest;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct EpicNPCTypeComparer : IEqualityComparer<EpicNPCType>
{
	public bool Equals(EpicNPCType x, EpicNPCType y)
	{
		return x == y;
	}

	public int GetHashCode(EpicNPCType x)
	{
		return (int)x;
	}
}
