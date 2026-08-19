using System.Collections.Generic;

namespace Shared.Faction;

public struct FactionTodoTypeComparer : IEqualityComparer<FactionTodoType>
{
	public bool Equals(FactionTodoType x, FactionTodoType y)
	{
		return x == y;
	}

	public int GetHashCode(FactionTodoType x)
	{
		return (int)x;
	}
}
