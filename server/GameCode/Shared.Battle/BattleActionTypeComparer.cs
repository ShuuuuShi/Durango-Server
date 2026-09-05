using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Battle;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct BattleActionTypeComparer : IEqualityComparer<BattleActionType>
{
	public bool Equals(BattleActionType x, BattleActionType y)
	{
		return x == y;
	}

	public int GetHashCode(BattleActionType x)
	{
		return (int)x;
	}
}
