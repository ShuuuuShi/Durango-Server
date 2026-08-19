using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Yaml.Util;

public class SingletonDict<TK, TV> : Dictionary<TK, TV>, ISingletonable
{
	public static Dictionary<TK, TV> Instance { get; private set; }

	public void Initialize(object inst)
	{
		Instance = inst as Dictionary<TK, TV>;
	}

	public static TV Get(TK key, [Optional] TV defaultValue)
	{
		if (Instance == null)
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
