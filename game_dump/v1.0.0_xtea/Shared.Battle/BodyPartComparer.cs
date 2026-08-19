using System.Collections.Generic;

namespace Shared.Battle;

public struct BodyPartComparer : IEqualityComparer<BodyPart>
{
	public bool Equals(BodyPart x, BodyPart y)
	{
		return x == y;
	}

	public int GetHashCode(BodyPart x)
	{
		return (int)x;
	}
}
