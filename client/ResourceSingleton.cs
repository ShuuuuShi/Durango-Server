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
			return (T)null;
		}
		if (_instance != null)
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
			Debug.LogError($"{typeFromHandle.Name}: Empty Resource Path\n. Use <color=white>[<color=#4EC9B0FF>ResourcePath</color>(<color=#D69D62FF>\"Path\"</color>)]</color>");
			return (T)null;
		}
		T val = Resources.Load<T>(text);
		if (val == null)
		{
			_notFound = true;
			Debug.LogError($"{typeFromHandle.Name}: Wrong Resource Path - {text}");
			return (T)null;
		}
		_instance = val;
		return _instance;
	}
}
