using System;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Utils;

public class AsyncCachedData<T>
{
	private Action<T> _callback;

	private float? _validTime;

	private T _cachedValue;

	[NotNull]
	private readonly Action<T, Action<T>> _request;

	private readonly float _cachedDuration;

	public AsyncCachedData([NotNull] Action<T, Action<T>> request, float cacheDuration)
	{
		_request = request;
		_cachedDuration = cacheDuration;
	}

	public void Request([CanBeNull] Action<T> callback, bool ignoreCache = false)
	{
		if (ignoreCache)
		{
			_validTime = null;
		}
		float time = Time.time;
		if (_validTime.HasValue && time < _validTime.Value)
		{
			callback?.Invoke(_cachedValue);
			return;
		}
		bool num = _callback != null;
		_callback = (Action<T>)Delegate.Combine(_callback, callback);
		if (!num)
		{
			_request(_cachedValue, OnResponse);
		}
	}

	private void OnResponse(T data)
	{
		if (_cachedDuration > 0f)
		{
			_validTime = Time.time + _cachedDuration;
		}
		else
		{
			_validTime = null;
		}
		_cachedValue = data;
		Action<T> callback = _callback;
		_callback = null;
		callback?.Invoke(_cachedValue);
	}

	public T GetCachedValue()
	{
		return _cachedValue;
	}

	public void MarkAsDirty()
	{
		_validTime = null;
	}
}
