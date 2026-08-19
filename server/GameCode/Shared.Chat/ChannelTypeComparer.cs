using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Chat;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ChannelTypeComparer : IEqualityComparer<ChannelType>
{
	public bool Equals(ChannelType x, ChannelType y)
	{
		return x == y;
	}

	public int GetHashCode(ChannelType x)
	{
		return (int)x;
	}
}
