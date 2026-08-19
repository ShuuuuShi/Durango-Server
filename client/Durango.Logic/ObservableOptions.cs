using System;
using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;

namespace Durango.Logic;

public class ObservableOptions<T>
{
	private readonly Dictionary<string, Observable<T>> _values = new Dictionary<string, Observable<T>>();

	private readonly Dictionary<string, Action<T>> _cachedOnChanged = new Dictionary<string, Action<T>>();

	public void Set([NotNull] string key, T value)
	{
		Observable<T> observable = _values.Get(key);
		if (observable == null)
		{
			Add(key, value);
		}
		else
		{
			observable.Value = value;
		}
	}

	public T Get([NotNull] string key, T defaultValue)
	{
		Observable<T> observable = _values.Get(key);
		if (observable == null)
		{
			observable = Add(key, defaultValue);
		}
		return observable.Value;
	}

	[NotNull]
	private Observable<T> Add([NotNull] string key, T value)
	{
		Observable<T> observable = new Observable<T>(value);
		_values[key] = observable;
		if (_cachedOnChanged.TryGetValue(key, out var value2))
		{
			_cachedOnChanged.Remove(key);
			observable.Changed = (Action<T>)Delegate.Combine(observable.Changed, value2);
		}
		return observable;
	}

	public void AddOnChange([NotNull] string key, Action<T> onChange)
	{
		Observable<T> observable = _values.Get(key);
		if (observable == null)
		{
			Action<T> a = _cachedOnChanged.Get(key);
			a = (Action<T>)Delegate.Combine(a, onChange);
			_cachedOnChanged[key] = a;
		}
		else
		{
			observable.Changed = (Action<T>)Delegate.Combine(observable.Changed, onChange);
		}
	}
}
