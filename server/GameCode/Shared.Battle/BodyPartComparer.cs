using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
