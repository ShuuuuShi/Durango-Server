using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Faction;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct MissionTodoOrderComparer : IEqualityComparer<MissionTodoOrder>
{
	public bool Equals(MissionTodoOrder x, MissionTodoOrder y)
	{
		return x == y;
	}

	public int GetHashCode(MissionTodoOrder x)
	{
		return (int)x;
	}
}
