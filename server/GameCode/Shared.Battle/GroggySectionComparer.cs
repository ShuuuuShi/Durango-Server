using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct GroggySectionComparer : IEqualityComparer<GroggySection>
{
	public bool Equals(GroggySection x, GroggySection y)
	{
		return x == y;
	}

	public int GetHashCode(GroggySection x)
	{
		return (int)x;
	}
}
