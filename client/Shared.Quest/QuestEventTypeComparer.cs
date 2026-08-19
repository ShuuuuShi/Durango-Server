using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Quest;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct QuestEventTypeComparer : IEqualityComparer<QuestEventType>
{
	public bool Equals(QuestEventType x, QuestEventType y)
	{
		return x == y;
	}

	public int GetHashCode(QuestEventType x)
	{
		return (int)x;
	}
}
