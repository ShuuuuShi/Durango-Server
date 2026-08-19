using System;
using System.Collections.Generic;
using UnityEngine;

namespace Durango.UI;

public class DrawHistory
{
	private struct Item
	{
		public int Sequence;

		public int X;

		public int Y;

		public Color Prev;

		public Color Next;
	}

	private int _sequence;

	private int _cursor;

	private readonly List<Item> _list = new List<Item>();

	public event Action HistoryUpdated;

	public void Clear()
	{
		_sequence = 0;
		_cursor = 0;
		_list.Clear();
		if (this.HistoryUpdated != null)
		{
			this.HistoryUpdated();
		}
	}

	public bool HasHistory()
	{
		return _list.Count > 0;
	}

	public void Add(int x, int y, Color prev, Color next)
	{
		if (_cursor < _list.Count)
		{
			_list.RemoveRange(_cursor, _list.Count - _cursor);
		}
		_list.Add(new Item
		{
			Sequence = _sequence,
			X = x,
			Y = y,
			Prev = prev,
			Next = next
		});
		if (_list.Count > 8192)
		{
			int num = 4096;
			int sequence = _list[num].Sequence;
			for (int i = num; i < _list.Count; i++)
			{
				if (_list[i].Sequence != sequence)
				{
					num = i;
					break;
				}
			}
			_list.RemoveRange(0, num);
		}
		_cursor = _list.Count;
		if (this.HistoryUpdated != null)
		{
			this.HistoryUpdated();
		}
	}

	public void FinishSequence()
	{
		_sequence++;
	}

	public bool CanUndo()
	{
		return _cursor > 0;
	}

	public bool CanRedo()
	{
		return _cursor < _list.Count;
	}

	public void Undo(Texture2D canvas)
	{
		int num = _cursor - 1;
		if (num < 0 || num >= _list.Count)
		{
			return;
		}
		int sequence = _list[num].Sequence;
		for (int num2 = num; num2 >= 0; num2--)
		{
			Item item = _list[num2];
			if (item.Sequence != sequence)
			{
				break;
			}
			_cursor = num2;
			canvas.SetPixel(item.X, item.Y, item.Prev);
		}
		canvas.Apply();
		if (this.HistoryUpdated != null)
		{
			this.HistoryUpdated();
		}
	}

	public void Redo(Texture2D canvas)
	{
		int cursor = _cursor;
		if (cursor < 0 || cursor >= _list.Count)
		{
			return;
		}
		int sequence = _list[cursor].Sequence;
		for (int i = cursor; i < _list.Count; i++)
		{
			Item item = _list[i];
			if (item.Sequence != sequence)
			{
				break;
			}
			_cursor = i + 1;
			canvas.SetPixel(item.X, item.Y, item.Next);
		}
		canvas.Apply();
		if (this.HistoryUpdated != null)
		{
			this.HistoryUpdated();
		}
	}
}
