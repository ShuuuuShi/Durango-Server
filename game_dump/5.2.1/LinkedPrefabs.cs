using System;
using System.Collections.Generic;
using Durango.System;
using Durango.UI;
using JetBrains.Annotations;
using UnityEngine;

public class LinkedPrefabs
{
	private readonly GameObject _parent;

	private readonly IList<GameObject> _prefabs;

	private readonly Dictionary<Type, Component> _cachedScript = new Dictionary<Type, Component>();

	private readonly DictionaryIgnoreCase<IUriInvokable> _uriInvoker = new DictionaryIgnoreCase<IUriInvokable>();

	private Action<GameObject> _initializer;

	private Func<GameObject, bool> _condition;

	public LinkedPrefabs(GameObject parent, IList<GameObject> prefabs)
	{
		_parent = parent;
		_prefabs = prefabs;
	}

	public void Load(Action<GameObject> initializer, Func<GameObject, bool> condition)
	{
		if (_parent == null || _prefabs == null)
		{
			return;
		}
		_initializer = initializer;
		_condition = condition;
		Transform transform = _parent.transform;
		int i = 0;
		for (int size = KUtility.GetSize(_prefabs); i < size; i++)
		{
			GameObject gameObject = _prefabs[i];
			if (!(gameObject == null) && (_condition == null || _condition(gameObject)))
			{
				Transform transform2 = transform.Find(GetUIPrefabName(gameObject.name));
				if (transform2 == null)
				{
					transform2 = AddChild(gameObject, transform).transform;
				}
				InitializeObject(transform2.gameObject);
			}
		}
	}

	private static GameObject AddChild([NotNull] GameObject o, Transform parent)
	{
		GameObject gameObject = UnityEngine.Object.Instantiate(o, parent);
		gameObject.SetActive(value: true);
		gameObject.transform.localPosition = o.transform.localPosition;
		gameObject.transform.localScale = o.transform.localScale;
		gameObject.name = GetUIPrefabName(o.name);
		return gameObject;
	}

	private static string GetUIPrefabName(string name)
	{
		if (!Platform.Instance.UsePCUI)
		{
			return name;
		}
		if (name.EndsWith("_PC", StringComparison.InvariantCultureIgnoreCase))
		{
			return name.Substring(0, name.Length - 3);
		}
		return name;
	}

	public T FindScript<T>() where T : Component
	{
		Type typeFromHandle = typeof(T);
		if (_cachedScript.TryGetValue(typeFromHandle, out var value))
		{
			return value as T;
		}
		Transform transform = _parent.transform;
		int i = 0;
		for (int childCount = transform.childCount; i < childCount; i++)
		{
			T component = transform.GetChild(i).GetComponent<T>();
			if (component != null)
			{
				_cachedScript.Add(typeFromHandle, component);
				return component;
			}
		}
		int j = 0;
		for (int size = KUtility.GetSize(_prefabs); j < size; j++)
		{
			GameObject gameObject = _prefabs[j];
			if (!(gameObject == null) && !(gameObject.GetComponent<T>() == null))
			{
				GameObject gameObject2 = AddChild(gameObject, transform);
				InitializeObject(gameObject2);
				T component2 = gameObject2.GetComponent<T>();
				_cachedScript.Add(typeFromHandle, component2);
				return component2;
			}
		}
		return null;
	}

	private void InitializeObject(GameObject obj)
	{
		IUriInvokable component = obj.GetComponent<IUriInvokable>();
		if (component != null)
		{
			object[] customAttributes = component.GetType().GetCustomAttributes(typeof(UriAttribute), inherit: false);
			for (int i = 0; i < customAttributes.Length; i++)
			{
				string key = ((UriAttribute)customAttributes[i]).Key;
				_uriInvoker[key] = component;
			}
		}
		if (_initializer != null)
		{
			_initializer(obj);
		}
	}

	public IUriInvokable FindUriInvoker(string key)
	{
		return _uriInvoker.Get(key);
	}

	public IEnumerable<KeyValuePair<string, IUriInvokable>> GetUriInvokers()
	{
		return _uriInvoker;
	}
}
