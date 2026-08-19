using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Durango.UI;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct CordinateComparer : IEqualityComparer<Point2>
{
	public bool Equals(Point2 a, Point2 b)
	{
		return a.x == b.x && a.y == b.y;
	}

	public int GetHashCode(Point2 value)
	{
		return ((long)value.x.GetHashCode() + (long)value.y.GetHashCode()).GetHashCode();
	}
}
