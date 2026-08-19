using System;
using UnityEngine;

public abstract class ResourceSingleton<T> : ScriptableObject where T : ScriptableObject
{
	private static T _instance;

	private static bool _notFound;

	public static T Instance()
	{
		if (_notFound)
		{
			return (T)(object)null;
		}
		if ((Object)(object)_instance != (Object)null)
		{
			return _instance;
		}
		Type typeFromHandle = typeof(T);
		object[] customAttributes = typeFromHandle.GetCustomAttributes(typeof(ResourcePathAttribute), inherit: true);
		string text = null;
		int i = 0;
		for (int num = customAttributes.Length; i < num; i++)
		{
			if (customAttributes[i] is ResourcePathAttribute resourcePathAttribute)
			{
				text = resourcePathAttribute.Path;
			}
		}
		if (string.IsNullOrEmpty(text))
		{
			_notFound = true;
			return (T)(object)null;
		}
		T val = Resources.Load<T>(text);
		if ((Object)(object)val == (Object)null)
		{
			_notFound = true;
			return (T)(object)null;
		}
		_instance = val;
		return _instance;
	}
}
