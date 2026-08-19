using System;
using System.Collections.Generic;

namespace Durango.Utils;

public class Reusable<T> : IDisposable where T : class, new()
{
	private static readonly Stack<Reusable<T>> Pool = new Stack<Reusable<T>>();

	public readonly T Value = new T();

	protected Reusable()
	{
	}

	public void Dispose()
	{
		Pool.Push(this);
	}

	protected static Reusable<T> DoPop()
	{
		if (Pool.Count == 0)
		{
			return new Reusable<T>();
		}
		return Pool.Pop();
	}

	public static implicit operator T(Reusable<T> reusable)
	{
		return reusable.Value;
	}
}
