using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

public class StaticObjectPool : KSingleton<StaticObjectPool>
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
			value2.SetActive(true);
			return value2;
		}
		return null;
	}

	public void ReturnObject(string poolName, [CanBeNull] GameObject obj)
	{
		if (string.IsNullOrEmpty(poolName) || (Object)(object)obj == (Object)null)
		{
			return;
		}
		ClientAnimalActor component = obj.GetComponent<ClientAnimalActor>();
		if ((Object)(object)component != (Object)null)
		{
			Object.Destroy((Object)(object)obj);
			return;
		}
		if (!_poolDict.TryGetValue(poolName, out var value))
		{
			value = new ObjectPool();
			_poolDict.Add(poolName, value);
		}
		value.PooledObjects.AddLast(obj);
		obj.SetActive(false);
	}
}
