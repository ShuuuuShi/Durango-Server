using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public abstract class ListObjectPoolBase<T> : IEnumerable<T>, IEnumerable where T : UnityEngine.Object
{
	[Serializable]
	public struct Enumerator : IEnumerator<T>, IEnumerator, IDisposable
	{
		private readonly ListObjectPoolBase<T> _list;

		private int _index;

		private T _current;

		object IEnumerator.Current => Current;

		public T Current => _current;

		internal Enumerator(ListObjectPoolBase<T> list)
		{
			_list = list;
			_index = 0;
			_current = (T)null;
		}

		public void Dispose()
		{
		}

		public bool MoveNext()
		{
			ListObjectPoolBase<T> list = _list;
			if (_index < list.Count)
			{
				_current = list[_index];
				_index++;
				return true;
			}
			_index = list.Count;
			_current = (T)null;
			return false;
		}

		void IEnumerator.Reset()
		{
			_index = 0;
			_current = (T)null;
		}
	}

	private Action<T> _objectInitializer;

	private readonly List<T> _list = new List<T>();

	private bool _initialized;

	private int _loadCount;

	public virtual T BaseObject { get; set; }

	public virtual bool UseBase { get; set; }

	public virtual Transform Parent { get; private set; }

	public int Count { get; private set; }

	public T this[int index] => _list[index];

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}

	IEnumerator<T> IEnumerable<T>.GetEnumerator()
	{
		return GetEnumerator();
	}

	public Enumerator GetEnumerator()
	{
		return new Enumerator(this);
	}

	public void Init(Action<T> objectInitialize, Transform parent = null)
	{
		if (_initialized || BaseObject == null)
		{
			return;
		}
		_initialized = true;
		Parent = ((!(parent == null)) ? parent : GetComponent<Transform>(BaseObject).parent);
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

	public T GetOrAdd(int index)
	{
		T node = GetNode(index);
		if (index >= Count)
		{
			Count = index + 1;
		}
		return node;
	}

	public TK Get<TK>(int index) where TK : Component
	{
		T val = this[index];
		return (!(val == null)) ? GetComponent<TK>(val) : ((TK)null);
	}

	public void Clear()
	{
		Set(0);
	}

	public void Set(int count)
	{
		Init(null);
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
		T node = GetNode(Count++);
		SetActive(node, active: true);
		return node;
	}

	public TK Add<TK>() where TK : Component
	{
		T val = Add();
		return (!(val == null)) ? GetComponent<TK>(val) : ((TK)null);
	}

	public void Remove(int index)
	{
		if (index >= 0 && index < _list.Count)
		{
			T val = _list[index];
			SetActive(val, active: false);
			_list.RemoveAt(index);
			_list.Add(val);
			Count--;
		}
	}

	private T GetNode(int index)
	{
		if (index < 0)
		{
			return (T)null;
		}
		Init(null);
		while (_list.Count <= index)
		{
			MakeNew(out var obj, out var comp);
			GameObject gameObject = obj;
			T baseObject = BaseObject;
			gameObject.name = $"{baseObject.name}_{_list.Count}";
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
			if (_list[i] == obj)
			{
				return i;
			}
		}
		return -1;
	}

	public float Reposition(Vector3 dir, int margin = 0)
	{
		if (BaseObject == null)
		{
			return 0f;
		}
		UIWidget component = GetComponent<UIWidget>(BaseObject);
		Vector3 vector = component.localCenter + component.transform.localPosition;
		Vector3 zero = Vector3.zero;
		zero.x = vector.x - (float)component.width * dir.x * 0.5f;
		zero.y = vector.y - (float)component.height * dir.y * 0.5f;
		return UIUtility.WidgetsReposition(_list, dir, zero, margin);
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

	public int GetLoadedCount()
	{
		return _loadCount;
	}

	public void BeginLoad()
	{
		_loadCount = 0;
	}

	public T GetNext()
	{
		return GetOrAdd(_loadCount++);
	}

	public void EndLoad()
	{
		Set(_loadCount);
	}

	protected abstract void SetActive(T obj, bool active);

	protected abstract void MakeNew(out GameObject obj, out T comp);

	protected abstract TK GetComponent<TK>(T obj) where TK : Component;
}
