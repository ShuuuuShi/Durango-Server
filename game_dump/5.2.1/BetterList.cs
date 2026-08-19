using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using UnityEngine;

public class BetterList<T> : IList<T>, ICollection<T>, IEnumerable<T>, IEnumerable
{
	public delegate int CompareFunc(T left, T right);

	[CompilerGenerated]
	private sealed class _003CGetEnumerator_003Ed__10 : IEnumerator<T>, IDisposable, IEnumerator
	{
		private int _003C_003E1__state;

		private T _003C_003E2__current;

		public BetterList<T> _003C_003E4__this;

		private int _003Ci_003E5__2;

		T IEnumerator<T>.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		object IEnumerator.Current
		{
			[DebuggerHidden]
			get
			{
				return _003C_003E2__current;
			}
		}

		[DebuggerHidden]
		public _003CGetEnumerator_003Ed__10(int _003C_003E1__state)
		{
			this._003C_003E1__state = _003C_003E1__state;
		}

		[DebuggerHidden]
		void IDisposable.Dispose()
		{
			_003C_003E1__state = -2;
		}

		[DebuggerStepThrough]
		private bool MoveNext()
		{
			int num = _003C_003E1__state;
			BetterList<T> betterList = _003C_003E4__this;
			if (num != 0)
			{
				if (num != 1)
				{
					return false;
				}
				_003C_003E1__state = -1;
				_003Ci_003E5__2++;
			}
			else
			{
				_003C_003E1__state = -1;
				if (betterList.buffer == null)
				{
					goto IL_0074;
				}
				_003Ci_003E5__2 = 0;
			}
			if (_003Ci_003E5__2 < betterList.size)
			{
				_003C_003E2__current = betterList.buffer[_003Ci_003E5__2];
				_003C_003E1__state = 1;
				return true;
			}
			goto IL_0074;
			IL_0074:
			return false;
		}

		bool IEnumerator.MoveNext()
		{
			//ILSpy generated this explicit interface implementation from .override directive in MoveNext
			return this.MoveNext();
		}

		[DebuggerHidden]
		void IEnumerator.Reset()
		{
			throw new NotSupportedException();
		}
	}

	public T[] buffer;

	public int size;

	[DebuggerHidden]
	public T this[int i]
	{
		get
		{
			return buffer[i];
		}
		set
		{
			buffer[i] = value;
		}
	}

	public int Count => size;

	public bool IsReadOnly => false;

	[DebuggerStepThrough]
	public IEnumerator<T> GetEnumerator()
	{
		//yield-return decompiler failed: Unexpected instruction in Iterator.Dispose()
		return new _003CGetEnumerator_003Ed__10(0)
		{
			_003C_003E4__this = this
		};
	}

	private void AllocateMore()
	{
		T[] array = ((buffer == null) ? new T[32] : new T[Mathf.Max(buffer.Length << 1, 32)]);
		if (buffer != null && size > 0)
		{
			buffer.CopyTo(array, 0);
		}
		buffer = array;
	}

	private void AllocateMore(int count)
	{
		T[] array = new T[count];
		if (buffer != null && size > 0)
		{
			buffer.CopyTo(array, 0);
		}
		buffer = array;
	}

	private void Trim()
	{
		if (size > 0)
		{
			if (size < buffer.Length)
			{
				T[] array = new T[size];
				for (int i = 0; i < size; i++)
				{
					array[i] = buffer[i];
				}
				buffer = array;
			}
		}
		else
		{
			buffer = null;
		}
	}

	public void Clear()
	{
		size = 0;
	}

	public void Release()
	{
		size = 0;
		buffer = null;
	}

	public void EnsureCapacity(int count)
	{
		if (buffer == null || count > buffer.Length)
		{
			AllocateMore(count);
		}
	}

	public void Add(T item)
	{
		if (buffer == null || size == buffer.Length)
		{
			AllocateMore();
		}
		buffer[size++] = item;
	}

	public void AddRange(T[] item, int length)
	{
		if (buffer == null || size + length > buffer.Length)
		{
			AllocateMore(size + length);
		}
		Array.Copy(item, 0, buffer, size, length);
		size += length;
	}

	public void Insert(int index, T item)
	{
		if (buffer == null || size == buffer.Length)
		{
			AllocateMore();
		}
		if (index > -1 && index < size)
		{
			for (int num = size; num > index; num--)
			{
				buffer[num] = buffer[num - 1];
			}
			buffer[index] = item;
			size++;
		}
		else
		{
			Add(item);
		}
	}

	public bool Contains(T item)
	{
		if (buffer == null)
		{
			return false;
		}
		for (int i = 0; i < size; i++)
		{
			if (buffer[i].Equals(item))
			{
				return true;
			}
		}
		return false;
	}

	public void CopyTo(T[] array, int arrayIndex)
	{
		if (size != 0)
		{
			Array.Copy(buffer, 0, array, arrayIndex, size);
		}
	}

	public int IndexOf(T item)
	{
		if (buffer == null)
		{
			return -1;
		}
		for (int i = 0; i < size; i++)
		{
			if (buffer[i].Equals(item))
			{
				return i;
			}
		}
		return -1;
	}

	public bool Remove(T item)
	{
		if (buffer != null)
		{
			EqualityComparer<T> @default = EqualityComparer<T>.Default;
			for (int i = 0; i < size; i++)
			{
				if (@default.Equals(buffer[i], item))
				{
					size--;
					buffer[i] = default(T);
					for (int j = i; j < size; j++)
					{
						buffer[j] = buffer[j + 1];
					}
					buffer[size] = default(T);
					return true;
				}
			}
		}
		return false;
	}

	public void RemoveAt(int index)
	{
		if (buffer != null && index > -1 && index < size)
		{
			size--;
			buffer[index] = default(T);
			for (int i = index; i < size; i++)
			{
				buffer[i] = buffer[i + 1];
			}
			buffer[size] = default(T);
		}
	}

	public void RemoveRange(int index, int count)
	{
		if (buffer != null && index > -1 && index < size)
		{
			count = Mathf.Min(count, size - index);
			size -= count;
			T val = default(T);
			for (int i = 0; i < count; i++)
			{
				buffer[index + i] = val;
			}
			for (int j = index; j < size; j++)
			{
				buffer[j] = buffer[j + count];
				buffer[j + count] = val;
			}
		}
	}

	public T Pop()
	{
		if (buffer != null && size != 0)
		{
			T result = buffer[--size];
			buffer[size] = default(T);
			return result;
		}
		return default(T);
	}

	public T[] ToArray()
	{
		Trim();
		return buffer;
	}

	[DebuggerHidden]
	[DebuggerStepThrough]
	public void Sort(CompareFunc comparer)
	{
		int num = 0;
		int num2 = size - 1;
		bool flag = true;
		while (flag)
		{
			flag = false;
			for (int i = num; i < num2; i++)
			{
				if (comparer(buffer[i], buffer[i + 1]) > 0)
				{
					T val = buffer[i];
					buffer[i] = buffer[i + 1];
					buffer[i + 1] = val;
					flag = true;
				}
				else if (!flag)
				{
					num = ((i != 0) ? (i - 1) : 0);
				}
			}
		}
	}

	IEnumerator IEnumerable.GetEnumerator()
	{
		return GetEnumerator();
	}
}
