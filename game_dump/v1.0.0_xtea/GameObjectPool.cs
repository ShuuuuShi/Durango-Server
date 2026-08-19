using System;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool<T> where T : Component
{
	private readonly T _baseObject;

	private readonly List<T> _pool;

	private readonly List<T> _list;

	private readonly Action<T> _initFunc;

	public int Count => _list.Count;

	public T this[int index] => _list[index];

	public List<T> List => _list;

	public List<T> Pool => _pool;

	public GameObjectPool(T baseObj, Action<T> initFunc = null)
	{
		_baseObject = baseObj;
		_initFunc = initFunc;
		((Component)_baseObject).gameObject.SetActive(false);
		_pool = new List<T>();
		_list = new List<T>();
	}

	public GameObject Get(int index)
	{
		return ((Component)_list[index]).gameObject;
	}

	public void Clear()
	{
		for (int i = 0; i < _list.Count; i++)
		{
			_pool.Add(_list[i]);
		}
		_list.Clear();
	}

	public T Pop()
	{
		return Insert(-1);
	}

	public T Insert(int index)
	{
		T val;
		if (_pool.Count > 0)
		{
			int index2 = _pool.Count - 1;
			val = _pool[index2];
			_pool.RemoveAt(index2);
		}
		else
		{
			val = Make();
		}
		if (index < 0 || index >= _list.Count)
		{
			_list.Add(val);
		}
		else
		{
			_list.Insert(index, val);
		}
		return val;
	}

	public T PushAt(int index)
	{
		if (index < 0 || index >= _list.Count)
		{
			return (T)(object)null;
		}
		T val = _list[index];
		_list.RemoveAt(index);
		_pool.Add(val);
		return val;
	}

	public void Push(T obj)
	{
		_list.Remove(obj);
		_pool.Add(obj);
	}

	private T Make()
	{
		T component = ((Component)((Component)_baseObject).transform.parent).gameObject.AddChild(((Component)_baseObject).gameObject).GetComponent<T>();
		if (_initFunc != null)
		{
			_initFunc(component);
		}
		return component;
	}
}
