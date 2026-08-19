using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Item;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
