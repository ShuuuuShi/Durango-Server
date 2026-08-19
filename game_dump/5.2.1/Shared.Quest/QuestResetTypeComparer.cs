using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Quest;

[StructLayout(LayoutKind.Sequential, Size = 1)]
public struct QuestResetTypeComparer : IEqualityComparer<QuestResetType>
{
	public bool Equals(QuestResetType x, QuestResetType y)
	{
		return x == y;
	}

	public int GetHashCode(QuestResetType x)
	{
		return (int)x;
	}
}
