using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
