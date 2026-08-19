using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Utils;

public class AsyncCachedDictionary<TK, TV>
{
	public delegate void RequestFunc(TK key, TV cachedValue, Action<TK, TV> onResult);

	public delegate void PostRequestDelegate(ref TV value);

	public delegate bool PreRequstDelegate(TK key, out TV value);

	private class CachedValue
	{
		public float ValidTime;

		public TV Value;
	}

	private static readonly EqualityComparer<TK> KeyComparer = EqualityComparer<TK>.Default;

	private readonly Dictionary<TK, CachedValue> _cachedValues = new Dictionary<TK, CachedValue>();

	private readonly Dictionary<TK, Action<TV>> _requestedDict = new Dictionary<TK, Action<TV>>();

	public PreRequstDelegate OnPreRequest;

	public PostRequestDelegate OnPostRequest;

	private readonly RequestFunc _requestFunc;

	public TK CurrentKey { get; private set; }

	public float CacheDuration { get; set; }

	public TK InvalidKey { get; set; }

	public TV EmptyValue { get; set; }

	public AsyncCachedDictionary([NotNull] RequestFunc func, float cacheDuration = 0f)
	{
		_requestFunc = func;
		CacheDuration = cacheDuration;
	}

	private bool IsInvalidKey([CanBeNull] TK key)
	{
		if (key != null)
		{
			return IsEqualKey(key, InvalidKey);
		}
		return true;
	}

	private static bool IsEqualKey(TK k1, TK k2)
	{
		return KeyComparer.Equals(k1, k2);
	}

	public void Request([CanBeNull] TK key, [NotNull] Action<TV> response, bool refresh = false)
	{
		if (IsInvalidKey(key))
		{
			_Response(key, EmptyValue, response);
			return;
		}
		if (OnPreRequest != null && OnPreRequest(key, out var value))
		{
			_Response(key, value, response);
			return;
		}
		bool flag = false;
		if (_cachedValues.TryGetValue(key, out var value2))
		{
			if (refresh || (value2.ValidTime > 0f && value2.ValidTime < Time.time))
			{
				flag = true;
			}
		}
		else
		{
			flag = true;
		}
		if (!flag)
		{
			_Response(key, value2.Value, response);
		}
		else if (_requestedDict.ContainsKey(key))
		{
			if (_requestedDict[key] == null)
			{
				_requestedDict[key] = response;
			}
			else
			{
				Dictionary<TK, Action<TV>> requestedDict;
				TK key2;
				(requestedDict = _requestedDict)[key2 = key] = (Action<TV>)Delegate.Combine(requestedDict[key2], response);
			}
		}
		else
		{
			_requestedDict[key] = response;
			_requestFunc(key, (value2 != null) ? value2.Value : default(TV), Response);
		}
	}

	public TV GetCachedValue([CanBeNull] TK key)
	{
		TryGetCachedValue(key, out var value);
		return value;
	}

	public bool TryGetCachedValue([CanBeNull] TK key, out TV value)
	{
		CachedValue cachedValue = _cachedValues.Get(key);
		if (cachedValue == null)
		{
			value = EmptyValue;
			return false;
		}
		value = cachedValue.Value;
		return true;
	}

	public void Refresh(IList<TK> keys)
	{
		int size = KUtility.GetSize(keys);
		for (int i = 0; i < size; i++)
		{
			TK key = keys[i];
			if (!IsInvalidKey(key))
			{
				_cachedValues.TryGetValue(key, out var value);
				_requestedDict[key] = null;
				_requestFunc(key, (value != null) ? value.Value : default(TV), Response);
			}
		}
	}

	public void Request(IList<TK> keys, [NotNull] Action<TV[]> response, bool refresh = false)
	{
		if (KUtility.GetSize(keys) == 0)
		{
			response(null);
			return;
		}
		TV[] responseList = new TV[keys.Count];
		bool[] responseCheck = new bool[keys.Count];
		int num = 0;
		for (int i = 0; i < responseList.Length; i++)
		{
			TK key = keys[i];
			if (IsInvalidKey(key))
			{
				responseList[i] = EmptyValue;
				responseCheck[i] = true;
				num++;
			}
		}
		if (num == keys.Count)
		{
			response(responseList);
			return;
		}
		Action<TV> response2 = delegate(TV info)
		{
			TK currentKey = CurrentKey;
			for (int k = 0; k < keys.Count; k++)
			{
				if (!responseCheck[k] && IsEqualKey(keys[k], currentKey))
				{
					responseList[k] = info;
					responseCheck[k] = true;
					break;
				}
			}
			for (int l = 0; l < responseCheck.Length; l++)
			{
				if (!responseCheck[l])
				{
					return;
				}
			}
			response(responseList);
		};
		int count = keys.Count;
		for (int j = 0; j < count; j++)
		{
			TK key2 = keys[j];
			if (!IsInvalidKey(key2))
			{
				Request(key2, response2, refresh);
			}
		}
	}

	public void SetValue([NotNull] TK key, TV value)
	{
		AddCache(key, value);
	}

	private void Response(TK key, TV value)
	{
		AddCache(key, value);
		Action<TV> response = _requestedDict.Get(key);
		_requestedDict.Remove(key);
		_Response(key, value, response);
	}

	private void _Response(TK key, TV value, Action<TV> response)
	{
		CurrentKey = key;
		if (OnPostRequest != null)
		{
			OnPostRequest(ref value);
		}
		response?.Invoke(value);
		CurrentKey = default(TK);
	}

	private void AddCache(TK key, TV value)
	{
		CachedValue cachedValue = _cachedValues.Get(key);
		if (cachedValue == null)
		{
			cachedValue = new CachedValue();
		}
		cachedValue.Value = value;
		cachedValue.ValidTime = ((!(CacheDuration > 0f)) ? 0f : (Time.time + CacheDuration));
		_cachedValues[key] = cachedValue;
	}
}
