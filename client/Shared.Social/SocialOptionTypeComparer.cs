using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Social;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SocialOptionTypeComparer : IEqualityComparer<SocialOptionType>
{
	public bool Equals(SocialOptionType x, SocialOptionType y)
	{
		return x == y;
	}

	public int GetHashCode(SocialOptionType x)
	{
		return (int)x;
	}
}
