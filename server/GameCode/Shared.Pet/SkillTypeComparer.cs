using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Pet;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SkillTypeComparer : IEqualityComparer<SkillType>
{
	public bool Equals(SkillType x, SkillType y)
	{
		return x == y;
	}

	public int GetHashCode(SkillType x)
	{
		return (int)x;
	}
}
