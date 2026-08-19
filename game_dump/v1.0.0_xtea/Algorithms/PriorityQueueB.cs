using System.Collections.Generic;

namespace Algorithms;

[Author("Franco, Gustavo")]
public class PriorityQueueB<T> : IPriorityQueue<T>
{
	protected List<T> InnerList = new List<T>();

	protected IComparer<T> mComparer;

	public int Count => InnerList.Count;

	public T this[int index]
	{
		get
		{
			return InnerList[index];
		}
		set
		{
			InnerList[index] = value;
			Update(index);
		}
	}

	public PriorityQueueB()
	{
		mComparer = Comparer<T>.Default;
	}

	public PriorityQueueB(IComparer<T> comparer)
	{
		mComparer = comparer;
	}

	public PriorityQueueB(IComparer<T> comparer, int capacity)
	{
		mComparer = comparer;
		InnerList.Capacity = capacity;
	}

	protected void SwitchElements(int i, int j)
	{
		T value = InnerList[i];
		InnerList[i] = InnerList[j];
		InnerList[j] = value;
	}

	protected virtual int OnCompare(int i, int j)
	{
		return mComparer.Compare(InnerList[i], InnerList[j]);
	}

	public int Push(T item)
	{
		int num = InnerList.Count;
		InnerList.Add(item);
		while (num != 0)
		{
			int num2 = (num - 1) / 2;
			if (OnCompare(num, num2) < 0)
			{
				SwitchElements(num, num2);
				num = num2;
				continue;
			}
			break;
		}
		return num;
	}

	public T Pop()
	{
		T result = InnerList[0];
		int num = 0;
		InnerList[0] = InnerList[InnerList.Count - 1];
		InnerList.RemoveAt(InnerList.Count - 1);
		while (true)
		{
			int num2 = num;
			int num3 = 2 * num + 1;
			int num4 = 2 * num + 2;
			if (InnerList.Count > num3 && OnCompare(num, num3) > 0)
			{
				num = num3;
			}
			if (InnerList.Count > num4 && OnCompare(num, num4) > 0)
			{
				num = num4;
			}
			if (num == num2)
			{
				break;
			}
			SwitchElements(num, num2);
		}
		return result;
	}

	public void Update(int i)
	{
		int num = i;
		while (num != 0)
		{
			int num2 = (num - 1) / 2;
			if (OnCompare(num, num2) < 0)
			{
				SwitchElements(num, num2);
				num = num2;
				continue;
			}
			break;
		}
		if (num < i)
		{
			return;
		}
		while (true)
		{
			int num3 = num;
			int num4 = 2 * num + 1;
			int num2 = 2 * num + 2;
			if (InnerList.Count > num4 && OnCompare(num, num4) > 0)
			{
				num = num4;
			}
			if (InnerList.Count > num2 && OnCompare(num, num2) > 0)
			{
				num = num2;
			}
			if (num == num3)
			{
				break;
			}
			SwitchElements(num, num3);
		}
	}

	public T Peek()
	{
		if (InnerList.Count > 0)
		{
			return InnerList[0];
		}
		return default(T);
	}

	public void Clear()
	{
		InnerList.Clear();
	}

	public void RemoveLocation(T item)
	{
		int num = -1;
		for (int i = 0; i < InnerList.Count; i++)
		{
			if (mComparer.Compare(InnerList[i], item) == 0)
			{
				num = i;
			}
		}
		if (num != -1)
		{
			InnerList.RemoveAt(num);
		}
	}
}
