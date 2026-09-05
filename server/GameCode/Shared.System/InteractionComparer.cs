using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.System;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct InteractionComparer : IEqualityComparer<Interaction>
{
	public bool Equals(Interaction x, Interaction y)
	{
		return x == y;
	}

	public int GetHashCode(Interaction x)
	{
		return (int)x;
	}
}
