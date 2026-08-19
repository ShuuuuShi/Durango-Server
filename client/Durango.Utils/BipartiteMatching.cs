using System.Collections.Generic;
using UnityEngine;

namespace Durango.Utils;

public class BipartiteMatching
{
	private class KeyNode
	{
		public bool Visited;

		public int LinkIndex;

		public List<int> Links { get; private set; }

		public int Value
		{
			get
			{
				if (Links == null || Links.Count == 0 || LinkIndex == -1)
				{
					return -1;
				}
				return Links[LinkIndex];
			}
		}

		public KeyNode()
		{
			Reset();
		}

		public void AddLink(int index)
		{
			List<int> list2 = Links ?? (Links = new List<int>());
			list2.Add(index);
		}

		public void Reset()
		{
			if (Links != null)
			{
				Links.Clear();
			}
			LinkIndex = -1;
			Visited = false;
		}
	}

	private struct StackValue
	{
		public int Index;

		public int LinkIndex;
	}

	private readonly List<KeyNode> _keys = new List<KeyNode>();

	private int _keyCount;

	private readonly List<int> _values = new List<int>();

	private readonly Stack<StackValue> _stack = new Stack<StackValue>();

	public void Reset()
	{
		_keyCount = 0;
		for (int i = 0; i < _keys.Count; i++)
		{
			_keys[i].Reset();
		}
		_values.Clear();
	}

	public void SetLink(int start, int end)
	{
		if (start >= _keys.Count)
		{
			for (int i = _keys.Count; i <= start; i++)
			{
				_keys.Add(new KeyNode());
			}
		}
		_keyCount = ((_keyCount <= start + 1) ? (start + 1) : _keyCount);
		if (end >= _values.Count)
		{
			for (int j = _values.Count; j <= end; j++)
			{
				_values.Add(-1);
			}
		}
		_keys[start].AddLink(end);
	}

	public int Match()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		int num = 0;
		for (int i = 0; i < _keyCount; i++)
		{
			if (Match(i))
			{
				num++;
			}
		}
		return num;
	}

	public int GetLink(int index)
	{
		if (index < 0 || index >= _keyCount)
		{
			return -1;
		}
		return _keys[index].Value;
	}

	public int GetRemainCount(int index)
	{
		if (index < 0 || index >= _keyCount)
		{
			return 0;
		}
		KeyNode keyNode = _keys[index];
		if (keyNode.Links == null)
		{
			return 0;
		}
		List<int> links = keyNode.Links;
		int num = 0;
		for (int i = 0; i < links.Count; i++)
		{
			int num2 = _values[links[i]];
			if (num2 == -1)
			{
				num++;
			}
		}
		return num;
	}

	private bool Match(int keyIndex)
	{
		KeyNode keyNode = _keys[keyIndex];
		if (keyNode.LinkIndex != -1)
		{
			return true;
		}
		if (keyNode.Links == null || keyNode.Links.Count == 0)
		{
			return false;
		}
		for (int i = 0; i < _keyCount; i++)
		{
			_keys[i].Visited = false;
		}
		Stack<StackValue> stack = _stack;
		stack.Clear();
		stack.Push(new StackValue
		{
			Index = keyIndex,
			LinkIndex = 0
		});
		while (stack.Count > 0)
		{
			StackValue stackValue = stack.Peek();
			KeyNode keyNode2 = _keys[stackValue.Index];
			if (keyNode2.LinkIndex == stackValue.LinkIndex || (keyNode2.Visited && stackValue.LinkIndex == 0))
			{
				keyNode2.Visited = true;
				stack.Pop();
				continue;
			}
			keyNode2.Visited = true;
			int num = _values[keyNode2.Links[stackValue.LinkIndex]];
			if (num == -1)
			{
				while (stack.Count > 0)
				{
					stackValue = stack.Pop();
					keyNode2 = _keys[stackValue.Index];
					keyNode2.LinkIndex = stackValue.LinkIndex;
					_values[keyNode2.Value] = stackValue.Index;
				}
				return true;
			}
			KeyNode keyNode3 = _keys[num];
			if (keyNode3.Visited)
			{
				stack.Pop();
				stack.Push(new StackValue
				{
					Index = stackValue.Index,
					LinkIndex = (stackValue.LinkIndex + 1) % keyNode2.Links.Count
				});
			}
			else
			{
				stack.Push(new StackValue
				{
					Index = num,
					LinkIndex = (keyNode3.LinkIndex + 1) % keyNode3.Links.Count
				});
			}
		}
		return false;
	}
}
