using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Region;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct WeatherComparer : IEqualityComparer<Weather>
{
	public bool Equals(Weather x, Weather y)
	{
		return x == y;
	}

	public int GetHashCode(Weather x)
	{
		return (int)x;
	}
}
