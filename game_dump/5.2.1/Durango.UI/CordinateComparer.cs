using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Durango.UI;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CordinateComparer : IEqualityComparer<Point2>
{
	public bool Equals(Point2 a, Point2 b)
	{
		if (a.x == b.x)
		{
			return a.y == b.y;
		}
		return false;
	}

	public int GetHashCode(Point2 value)
	{
		return ((long)value.x.GetHashCode() + (long)value.y.GetHashCode()).GetHashCode();
	}
}
