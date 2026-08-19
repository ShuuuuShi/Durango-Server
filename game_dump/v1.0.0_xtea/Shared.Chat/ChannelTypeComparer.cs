using System.Collections.Generic;

namespace Shared.Chat;

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
