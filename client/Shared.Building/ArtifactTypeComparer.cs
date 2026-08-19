using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Building;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
