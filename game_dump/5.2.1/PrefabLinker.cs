using System;
using System.Collections.Generic;
using Durango.UI;
using UnityEngine;

public class PrefabLinker : MonoBehaviour
{
	[SerializeField]
	[SortedUnityObjectList]
	private List<GameObject> _prefabs;

	private LinkedPrefabs _linkedPrefabs;

	public void Load(Action<GameObject> initializer, Func<GameObject, bool> condition)
	{
		if (_linkedPrefabs == null)
		{
			_linkedPrefabs = new LinkedPrefabs(base.gameObject, _prefabs);
		}
		_linkedPrefabs.Load(initializer, condition);
	}

	public T FindScript<T>() where T : Component
	{
		if (_linkedPrefabs == null)
		{
			return null;
		}
		return _linkedPrefabs.FindScript<T>();
	}

	public IUriInvokable FindUriInvoker(string key)
	{
		if (_linkedPrefabs == null)
		{
			return null;
		}
		return _linkedPrefabs.FindUriInvoker(key);
	}

	public IEnumerable<KeyValuePair<string, IUriInvokable>> GetUriInvokers()
	{
		if (_linkedPrefabs == null)
		{
			return null;
		}
		return _linkedPrefabs.GetUriInvokers();
	}
}
