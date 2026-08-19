using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Voucher;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GuideTypeComparer : IEqualityComparer<GuideType>
{
	public bool Equals(GuideType x, GuideType y)
	{
		return x == y;
	}

	public int GetHashCode(GuideType x)
	{
		return (int)x;
	}
}
