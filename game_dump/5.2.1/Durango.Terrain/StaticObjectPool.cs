using System.Collections.Generic;
using Durango.Utils;
using JetBrains.Annotations;
using UnityEngine;

namespace Durango.Terrain;

public class StaticObjectPool : Singleton<StaticObjectPool>
{
	private class ObjectPool
	{
		public readonly LinkedList<GameObject> PooledObjects = new LinkedList<GameObject>();
	}

	private readonly Dictionary<string, ObjectPool> _poolDict = new Dictionary<string, ObjectPool>();

	public GameObject RequestObject(string poolName)
	{
		if (string.IsNullOrEmpty(poolName))
		{
			return null;
		}
		if (_poolDict.TryGetValue(poolName, out var value) && value.PooledObjects.Count > 0)
		{
			GameObject value2 = value.PooledObjects.First.Value;
			value.PooledObjects.RemoveFirst();
			value2.SetActive(value: true);
			return value2;
		}
		return null;
	}

	public void ReturnObject(string poolName, [CanBeNull] GameObject obj)
	{
		if (string.IsNullOrEmpty(poolName) || obj == null)
		{
			return;
		}
		if (obj.GetComponent<ClientAnimalActor>() != null)
		{
			Object.Destroy(obj);
			return;
		}
		if (!_poolDict.TryGetValue(poolName, out var value))
		{
			value = new ObjectPool();
			_poolDict.Add(poolName, value);
		}
		value.PooledObjects.AddLast(obj);
		obj.SetActive(value: false);
	}
}
