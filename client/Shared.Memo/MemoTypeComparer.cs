using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Memo;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MemoTypeComparer : IEqualityComparer<MemoType>
{
	public bool Equals(MemoType x, MemoType y)
	{
		return x == y;
	}

	public int GetHashCode(MemoType x)
	{
		return (int)x;
	}
}
