using System;
using System.Collections.Generic;

namespace Durango.Utils;

public class Observable<T>
{
	public Action<T> Changed;

	private static readonly EqualityComparer<T> Comparer = EqualityComparer<T>.Default;

	private IEqualityComparer<T> _comparer;

	private T _value;

	public T Value
	{
		get
		{
			return _value;
		}
		set
		{
			if (_comparer != null)
			{
				if (_comparer.Equals(_value, value))
				{
					return;
				}
			}
			else if (Comparer.Equals(_value, value))
			{
				return;
			}
			_value = value;
			if (Changed != null)
			{
				Changed(_value);
			}
		}
	}

	public Observable(IEqualityComparer<T> comparer = null)
	{
		_value = default(T);
		_comparer = comparer;
	}

	public Observable(T value, IEqualityComparer<T> comparer = null)
	{
		_value = value;
		_comparer = comparer;
	}

	public static implicit operator T(Observable<T> value)
	{
		return value.Value;
	}
}
