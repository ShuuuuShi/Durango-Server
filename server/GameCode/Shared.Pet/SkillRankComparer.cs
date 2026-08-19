using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Pet;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SkillRankComparer : IEqualityComparer<SkillRank>
{
	public bool Equals(SkillRank x, SkillRank y)
	{
		return x == y;
	}

	public int GetHashCode(SkillRank x)
	{
		return (int)x;
	}
}
