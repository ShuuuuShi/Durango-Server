using System;
using System.Collections.Generic;
using UnityEngine;

public class SimpleContainer : MonoBehaviour
{
	public List<string> keys = new List<string>();

	public List<GameObject> values = new List<GameObject>();

	private Dictionary<string, GameObject> _dict;

	private Dictionary<KeyValuePair<GameObject, Type>, Component> _cacheDict;

	private Dictionary<string, object> _numberDict;

	public Dictionary<string, GameObject> Dict
	{
		get
		{
			if (_dict == null)
			{
				_dict = new Dictionary<string, GameObject>();
				for (int i = 0; i < keys.Count; i++)
				{
					_dict.Add(keys[i], values[i]);
				}
			}
			return _dict;
		}
	}

	private Dictionary<KeyValuePair<GameObject, Type>, Component> CacheDict
	{
		get
		{
			if (_cacheDict == null)
			{
				_cacheDict = new Dictionary<KeyValuePair<GameObject, Type>, Component>();
			}
			return _cacheDict;
		}
	}

	public Dictionary<string, object> NumberDict
	{
		get
		{
			if (_numberDict == null)
			{
				_numberDict = new Dictionary<string, object>();
			}
			return _numberDict;
		}
	}

	private GameObject GetGameObject(string key)
	{
		if (string.IsNullOrEmpty(key))
		{
			return ((Component)this).gameObject;
		}
		if (Dict.TryGetValue(key, out var value))
		{
			return value;
		}
		return null;
	}

	private object GetObject(string key, object defaultValue)
	{
		if (NumberDict.TryGetValue(key, out var value))
		{
			return value;
		}
		Set(key, defaultValue);
		return defaultValue;
	}

	public bool Has(string key)
	{
		if (Dict.ContainsKey(key))
		{
			return true;
		}
		if (NumberDict.ContainsKey(key))
		{
			return true;
		}
		return false;
	}

	public GameObject Get(string key = null)
	{
		return GetGameObject(key);
	}

	public T Get<T>(string key = null) where T : Component
	{
		GameObject gameObject = GetGameObject(key);
		if ((Object)(object)gameObject == (Object)null)
		{
			return (T)(object)null;
		}
		KeyValuePair<GameObject, Type> key2 = new KeyValuePair<GameObject, Type>(gameObject, typeof(T));
		if (CacheDict.TryGetValue(key2, out var value))
		{
			if (!((Object)(object)value == (Object)null))
			{
				return (T)(object)((value is T) ? value : null);
			}
			CacheDict.Remove(key2);
		}
		T component = gameObject.GetComponent<T>();
		if ((Object)(object)component == (Object)null)
		{
			return (T)(object)null;
		}
		CacheDict.Add(key2, (Component)(object)component);
		return component;
	}

	public T GetValue<T>(string key)
	{
		return GetValue(key, default(T));
	}

	public T GetValue<T>(string key, T defaultValue)
	{
		object @object = GetObject(key, defaultValue);
		if (@object is T)
		{
			return (T)@object;
		}
		return defaultValue;
	}

	public void Set(string key, object obj)
	{
		Component val = (Component)((obj is Component) ? obj : null);
		if ((Object)(object)val != (Object)null)
		{
			if (Dict.ContainsKey(key))
			{
				Dict[key] = val.gameObject;
			}
			else
			{
				Dict.Add(key, val.gameObject);
			}
		}
		else if (NumberDict.ContainsKey(key))
		{
			NumberDict[key] = obj;
		}
		else
		{
			NumberDict.Add(key, obj);
		}
	}
}
