using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ColorExtractionTypeComparer : IEqualityComparer<ColorExtractionType>
{
	public bool Equals(ColorExtractionType x, ColorExtractionType y)
	{
		return x == y;
	}

	public int GetHashCode(ColorExtractionType x)
	{
		return (int)x;
	}
}
