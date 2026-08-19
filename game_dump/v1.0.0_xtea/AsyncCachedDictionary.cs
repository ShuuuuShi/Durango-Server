using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class AsyncCachedDictionary<TK, TV>
{
	private class CachedValue
	{
		public float ValidTime;

		public TV Value;
	}

	public delegate void RequestFunc(TK key, TV cachedValue, Action<TK, TV> onResult);

	public delegate void PostRequestDelegate(ref TV value);

	private static readonly EqualityComparer<TK> KeyComparer = EqualityComparer<TK>.Default;

	private static readonly EqualityComparer<TV> ValueComparer = EqualityComparer<TV>.Default;

	public static TK CurrentKey;

	private readonly Dictionary<TK, CachedValue> _cachedValues = new Dictionary<TK, CachedValue>();

	private readonly Dictionary<TK, Action<TV>> _requestedDict = new Dictionary<TK, Action<TV>>();

	public PostRequestDelegate OnPostRequest;

	private readonly RequestFunc _requestFunc;

	public float CacheDuration { get; set; }

	public TK InvalidKey { get; set; }

	public TV EmptyValue { get; set; }

	public AsyncCachedDictionary([NotNull] RequestFunc func, float cacheDuration = 0f)
	{
		_requestFunc = func;
		CacheDuration = cacheDuration;
	}

	private bool IsInvalidKey(TK key)
	{
		return IsEqualKey(key, InvalidKey);
	}

	private bool IsEqualKey(TK k1, TK k2)
	{
		return KeyComparer.Equals(k1, k2);
	}

	private bool IsEqualValue(TV v1, TV v2)
	{
		return ValueComparer.Equals(v1, v2);
	}

	public TV GetCachedValue(TK key)
	{
		CachedValue cachedValue = _cachedValues.Get(key);
		if (cachedValue == null)
		{
			return EmptyValue;
		}
		return cachedValue.Value;
	}

	public void Request(TK key, [NotNull] Action<TV> response, bool refresh = false)
	{
		if (IsEqualKey(key, InvalidKey))
		{
			_Response(key, EmptyValue, response);
			return;
		}
		bool flag = false;
		if (_cachedValues.TryGetValue(key, out var value))
		{
			if (refresh || (value.ValidTime > 0f && value.ValidTime < Time.time))
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
			_Response(key, value.Value, response);
		}
		else if (_requestedDict.ContainsKey(key))
		{
			Dictionary<TK, Action<TV>> requestedDict;
			Dictionary<TK, Action<TV>> dictionary = (requestedDict = _requestedDict);
			TK key2;
			TK key3 = (key2 = key);
			Action<TV> a = requestedDict[key2];
			dictionary[key3] = (Action<TV>)Delegate.Combine(a, response);
		}
		else
		{
			_requestedDict[key] = response;
			_requestFunc(key, (value != null) ? value.Value : default(TV), Response);
		}
	}

	public void Request(IList<TK> keys, [NotNull] Action<TV[]> response, bool refresh = false)
	{
		if (keys == null || keys.Count == 0)
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

	public void SetValue(TK key, TV value)
	{
		AddCache(key, value);
	}

	private void Response(TK key, TV value)
	{
		if (IsEqualValue(value, default(TV)))
		{
			value = EmptyValue;
		}
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
