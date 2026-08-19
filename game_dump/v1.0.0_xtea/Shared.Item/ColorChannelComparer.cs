using System.Collections.Generic;

namespace Shared.Item;

public struct ColorChannelComparer : IEqualityComparer<ColorChannel>
{
	public bool Equals(ColorChannel x, ColorChannel y)
	{
		return x == y;
	}

	public int GetHashCode(ColorChannel x)
	{
		return (int)x;
	}
}
