using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Yaml.Util;

public class SingletonDict<TK, TV> : Dictionary<TK, TV>, ISingletonable
{
	public static Dictionary<TK, TV> Instance { get; private set; }

	private static event Action Initalized;

	public void Initialize(object inst)
	{
		Instance = inst as Dictionary<TK, TV>;
		OnInitalized();
		if (SingletonDict<TK, TV>.Initalized != null)
		{
			SingletonDict<TK, TV>.Initalized();
		}
		SingletonDict<TK, TV>.Initalized = null;
	}

	protected virtual void OnInitalized()
	{
	}

	[CanBeNull]
	public static TV Get([CanBeNull] TK key, TV defaultValue = default(TV))
	{
		if (Instance == null || key == null)
		{
			return defaultValue;
		}
		TV value;
		return (!Instance.TryGetValue(key, out value)) ? defaultValue : value;
	}

	public new static bool TryGetValue(TK key, out TV value)
	{
		if (Instance == null)
		{
			value = default(TV);
			return false;
		}
		return Instance.TryGetValue(key, out value);
	}
}
