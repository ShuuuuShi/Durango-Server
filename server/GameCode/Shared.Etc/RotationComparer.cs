using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Etc;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct RotationComparer : IEqualityComparer<Rotation>
{
	public bool Equals(Rotation x, Rotation y)
	{
		return x == y;
	}

	public int GetHashCode(Rotation x)
	{
		return (int)x;
	}
}
