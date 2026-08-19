using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Pet;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct SkillTargetComparer : IEqualityComparer<SkillTarget>
{
	public bool Equals(SkillTarget x, SkillTarget y)
	{
		return x == y;
	}

	public int GetHashCode(SkillTarget x)
	{
		return (int)x;
	}
}
