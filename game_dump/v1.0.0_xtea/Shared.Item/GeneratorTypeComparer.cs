using System.Collections.Generic;

namespace Shared.Item;

public struct GeneratorTypeComparer : IEqualityComparer<GeneratorType>
{
	public bool Equals(GeneratorType x, GeneratorType y)
	{
		return x == y;
	}

	public int GetHashCode(GeneratorType x)
	{
		return (int)x;
	}
}
