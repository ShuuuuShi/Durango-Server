using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Pet;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SkillContextComparer : IEqualityComparer<SkillContext>
{
	public bool Equals(SkillContext x, SkillContext y)
	{
		return x == y;
	}

	public int GetHashCode(SkillContext x)
	{
		return (int)x;
	}
}
