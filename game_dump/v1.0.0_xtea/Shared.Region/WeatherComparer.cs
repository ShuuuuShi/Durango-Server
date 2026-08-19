using System.Collections.Generic;

namespace Shared.Region;

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
