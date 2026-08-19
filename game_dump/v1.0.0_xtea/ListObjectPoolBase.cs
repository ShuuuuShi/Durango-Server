using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ListObjectPoolBase<T> where T : Object
{
	private Action<T> _objectInitializer;

	private readonly List<T> _list = new List<T>();

	private bool _initialized;

	public virtual T BaseObject { get; set; }

	public virtual bool UseBase { get; set; }

	public int Count { get; private set; }

	public T this[int index] => (index >= 0) ? _list[index] : _list[Count + index];

	public void Init(Action<T> objectInitialize)
	{
		if (_initialized)
		{
			return;
		}
		_initialized = true;
		_objectInitializer = objectInitialize;
		SetActive(BaseObject, active: false);
		if (UseBase)
		{
			if (_objectInitializer != null)
			{
				_objectInitializer(BaseObject);
			}
			_list.Add(BaseObject);
		}
	}

	public T Get(int index)
	{
		return this[index];
	}

	public TK Get<TK>(int index) where TK : Component
	{
		T val = Get(index);
		return (!((Object)(object)val == (Object)null)) ? GetComponent<TK>(val) : ((TK)(object)null);
	}

	public void Clear()
	{
		Set(0);
	}

	public void Set(int count)
	{
		if (!_initialized)
		{
			Init(null);
		}
		for (int i = 0; i < count; i++)
		{
			T node = GetNode(i);
			SetActive(node, active: true);
		}
		for (int j = count; j < _list.Count; j++)
		{
			T obj = _list[j];
			SetActive(obj, active: false);
		}
		Count = count;
	}

	public T Add()
	{
		return Insert(Count);
	}

	public TK Add<TK>() where TK : Component
	{
		return Insert<TK>(Count);
	}

	public T Insert(int index)
	{
		if (!_initialized)
		{
			Init(null);
		}
		T node = GetNode(Count++);
		if (index >= 0 && index < Count - 1)
		{
			int index2 = Count - 1;
			T value = _list[index];
			_list[index] = _list[index2];
			_list[index2] = value;
		}
		SetActive(node, active: true);
		return node;
	}

	public TK Insert<TK>(int index) where TK : Component
	{
		T val = Insert(index);
		return (!((Object)(object)val == (Object)null)) ? GetComponent<TK>(val) : ((TK)(object)null);
	}

	public void Remove(int index)
	{
		Swap(index, Count - 1);
		Set(Mathf.Max(0, Count - 1));
	}

	protected T GetNode(int index)
	{
		if (index < 0)
		{
			return (T)(object)null;
		}
		while (_list.Count <= index)
		{
			MakeNew(out var obj, out var comp);
			((Object)obj).name = $"{((Object)BaseObject).name}_{_list.Count}";
			if (_objectInitializer != null)
			{
				_objectInitializer(comp);
			}
			_list.Add(comp);
		}
		return _list[index];
	}

	public int IndexOf(T obj)
	{
		int i = 0;
		for (int count = Count; i < count; i++)
		{
			if ((Object)(object)this[i] == (Object)(object)obj)
			{
				return i;
			}
		}
		return -1;
	}

	public float Reposition(Vector3 dir, int margin = 0)
	{
		//IL_0012: Unknown result type (might be due to invalid IL or missing references)
		return UIUtility.WidgetsReposition(Get, Count, dir, GetComponent<UIWidget>(BaseObject), margin);
	}

	public bool Swap(int i1, int i2)
	{
		if (i1 == i2 || i1 < 0 || i1 >= _list.Count || i2 < 0 || i2 >= _list.Count)
		{
			return false;
		}
		T value = _list[i1];
		_list[i1] = _list[i2];
		_list[i2] = value;
		return true;
	}

	protected abstract void SetActive(T obj, bool active);

	protected abstract void MakeNew(out GameObject obj, out T comp);

	protected abstract TK GetComponent<TK>(T obj) where TK : Component;
}
