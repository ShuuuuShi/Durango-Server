using System.Collections.Generic;

namespace Shared.Item;

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
