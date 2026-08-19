using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Quest;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct QuestTypeComparer : IEqualityComparer<QuestType>
{
	public bool Equals(QuestType x, QuestType y)
	{
		return x == y;
	}

	public int GetHashCode(QuestType x)
	{
		return (int)x;
	}
}
