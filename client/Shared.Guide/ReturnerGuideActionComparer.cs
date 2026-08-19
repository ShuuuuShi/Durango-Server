using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Guide;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ReturnerGuideActionComparer : IEqualityComparer<ReturnerGuideAction>
{
	public bool Equals(ReturnerGuideAction x, ReturnerGuideAction y)
	{
		return x == y;
	}

	public int GetHashCode(ReturnerGuideAction x)
	{
		return (int)x;
	}
}
