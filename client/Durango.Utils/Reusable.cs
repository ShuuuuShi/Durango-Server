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
		return (Pool.Count != 0) ? Pool.Pop() : new Reusable<T>();
	}

	public static implicit operator T(Reusable<T> reusable)
	{
		return reusable.Value;
	}
}
