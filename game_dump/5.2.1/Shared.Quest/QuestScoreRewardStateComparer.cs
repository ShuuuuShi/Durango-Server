using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Quest;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct QuestScoreRewardStateComparer : IEqualityComparer<QuestScoreRewardState>
{
	public bool Equals(QuestScoreRewardState x, QuestScoreRewardState y)
	{
		return x == y;
	}

	public int GetHashCode(QuestScoreRewardState x)
	{
		return (int)x;
	}
}
