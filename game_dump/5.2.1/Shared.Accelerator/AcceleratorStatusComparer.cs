using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Accelerator;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct AcceleratorStatusComparer : IEqualityComparer<AcceleratorStatus>
{
	public bool Equals(AcceleratorStatus x, AcceleratorStatus y)
	{
		return x == y;
	}

	public int GetHashCode(AcceleratorStatus x)
	{
		return (int)x;
	}
}
