using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Quest;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct QuestStateComparer : IEqualityComparer<QuestState>
{
	public bool Equals(QuestState x, QuestState y)
	{
		return x == y;
	}

	public int GetHashCode(QuestState x)
	{
		return (int)x;
	}
}
