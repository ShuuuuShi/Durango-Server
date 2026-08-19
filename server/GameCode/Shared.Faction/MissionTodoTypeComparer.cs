using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Faction;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MissionTodoTypeComparer : IEqualityComparer<MissionTodoType>
{
	public bool Equals(MissionTodoType x, MissionTodoType y)
	{
		return x == y;
	}

	public int GetHashCode(MissionTodoType x)
	{
		return (int)x;
	}
}
