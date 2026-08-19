using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Building;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct ArtifactStatEffectComparer : IEqualityComparer<ArtifactStatEffect>
{
	public bool Equals(ArtifactStatEffect x, ArtifactStatEffect y)
	{
		return x == y;
	}

	public int GetHashCode(ArtifactStatEffect x)
	{
		return (int)x;
	}
}
