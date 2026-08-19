using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Shared.Guide;

[StructLayout(LayoutKind.Sequential, Size = 1)]
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
