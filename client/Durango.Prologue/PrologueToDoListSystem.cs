using System;
using System.Collections.Generic;
using Durango.Logic.PlayGuide;

namespace Durango.Prologue;

public class PrologueToDoListSystem : GameSystem<PrologueToDoListSystem>
{
	private readonly List<ToDoBase> _todoList = new List<ToDoBase>();

	public event Action<List<ToDoBase>, bool> ListUpdated;

	public event Action<ToDoBase> ProgressUpdated;

	public event Action<ToDoBase> TextUpdated;

	public event Action<ToDoBase> CompletionUpdated;

	private void Update()
	{
		for (int num = _todoList.Count - 1; num >= 0; num--)
		{
			if (!_todoList[num].IsCompleted)
			{
				_todoList[num].Process();
			}
		}
	}

	public void AddToDoItems(List<ToDoBase> toDoItems)
	{
		int count = toDoItems.Count;
		for (int i = 0; i < count; i++)
		{
			ToDoBase item = toDoItems[i];
			AddToDoItemsInternal(item);
		}
		OnListUpdated(added: true);
	}

	public void AddToDoItem(ToDoBase item)
	{
		AddToDoItemsInternal(item);
		OnListUpdated(added: true);
	}

	public void RemoveItem(ToDoBase toDoItem)
	{
		_todoList.Remove(toDoItem);
		toDoItem.OnRemoveItem();
		OnListUpdated();
	}

	public void RemoveItems(List<ToDoBase> toDoItems)
	{
		int count = toDoItems.Count;
		for (int i = 0; i < count; i++)
		{
			ToDoBase toDoBase = toDoItems[i];
			_todoList.Remove(toDoBase);
			toDoBase.OnRemoveItem();
		}
		OnListUpdated();
	}

	public void RemoveAll()
	{
		int count = _todoList.Count;
		for (int i = 0; i < count; i++)
		{
			ToDoBase toDoBase = _todoList[i];
			toDoBase.OnRemoveItem();
		}
		_todoList.Clear();
		OnListUpdated();
	}

	public ToDoBase FindToDo(string key)
	{
		for (int i = 0; i < _todoList.Count; i++)
		{
			if (_todoList[i].Key == key)
			{
				return _todoList[i];
			}
		}
		return null;
	}

	public void SetProgress(string key, int current)
	{
		ToDoBase toDoBase = FindToDo(key);
		if (toDoBase != null)
		{
			toDoBase.CurrentProgress = current;
			if (this.ProgressUpdated != null)
			{
				this.ProgressUpdated(toDoBase);
			}
		}
	}

	public void SetText(string key, string text)
	{
		ToDoBase toDoBase = FindToDo(key);
		if (toDoBase != null)
		{
			toDoBase.LocalText = text;
			if (this.TextUpdated != null)
			{
				this.TextUpdated(toDoBase);
			}
		}
	}

	public void SetCompleted(List<ToDoBase> toDoItems, bool completed)
	{
		int count = toDoItems.Count;
		for (int i = 0; i < count; i++)
		{
			ToDoBase toDoBase = toDoItems[i];
			SetCompleted(toDoBase.Key, completed);
		}
	}

	public void SetCompleted(string key, bool completed)
	{
		ToDoBase toDoBase = FindToDo(key);
		if (toDoBase != null)
		{
			toDoBase.IsCompleted = completed;
			if (this.CompletionUpdated != null)
			{
				this.CompletionUpdated(toDoBase);
			}
		}
	}

	public void CallComplete(string key)
	{
		FindToDo(key)?.CallComplete();
	}

	private void AddToDoItemsInternal(ToDoBase item)
	{
		if (FindToDo(item.Key) != null)
		{
			Debug.LogError("Duplicate key: " + item.Key);
			return;
		}
		_todoList.Add(item);
		item.OnAddItem();
	}

	private void OnListUpdated(bool added = false)
	{
		if (this.ListUpdated != null)
		{
			this.ListUpdated(_todoList, added);
		}
	}
}
