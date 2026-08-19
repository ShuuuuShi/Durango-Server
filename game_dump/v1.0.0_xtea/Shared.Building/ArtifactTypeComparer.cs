using System.Collections.Generic;

namespace Shared.Building;

public struct ArtifactTypeComparer : IEqualityComparer<ArtifactType>
{
	public bool Equals(ArtifactType x, ArtifactType y)
	{
		return x == y;
	}

	public int GetHashCode(ArtifactType x)
	{
		return (int)x;
	}
}
