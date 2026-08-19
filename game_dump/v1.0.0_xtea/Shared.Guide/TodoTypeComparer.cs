using System.Collections.Generic;

namespace Shared.Guide;

public struct TodoTypeComparer : IEqualityComparer<TodoType>
{
	public bool Equals(TodoType x, TodoType y)
	{
		return x == y;
	}

	public int GetHashCode(TodoType x)
	{
		return (int)x;
	}
}
