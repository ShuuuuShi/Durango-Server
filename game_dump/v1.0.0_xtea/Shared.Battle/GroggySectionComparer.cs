using System.Collections.Generic;

namespace Shared.Battle;

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
