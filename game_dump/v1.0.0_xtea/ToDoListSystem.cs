using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using PlayGuide;

public class ToDoListSystem : GameSystem<ToDoListSystem>
{
	private struct QueuedItem
	{
		public ToDoCollection Collection;

		public bool ForAdd;
	}

	private readonly List<ToDoCollection> _collections = new List<ToDoCollection>();

	private readonly List<QueuedItem> _queuedItems = new List<QueuedItem>();

	private float _lastRemovedTime;

	public List<ToDoCollection> Collections => _collections;

	public event Action<ToDoCollection, bool> Added;

	public event Action<ToDoCollection, bool> Removed;

	public event Action ListUpdated;

	public event Action<ToDoCollection, ToDoBase, bool> ContextUpdated;

	private void Update()
	{
		if (ProcessCollections())
		{
			ProcessQueuedItems();
		}
	}

	private bool ProcessCollections()
	{
		bool result = true;
		bool flag = false;
		for (int num = Collections.Count - 1; num >= 0; num--)
		{
			ToDoCollection toDoCollection = Collections[num];
			toDoCollection.Update();
			float tweenRatio = toDoCollection.TweenRatio;
			if (tweenRatio < 1f || toDoCollection.WillBeRemoved)
			{
				result = false;
			}
			if (tweenRatio < 0f)
			{
				Collections.RemoveAt(num);
				flag = true;
			}
		}
		if (flag)
		{
			OnListUpdated();
		}
		return result;
	}

	private void ProcessQueuedItems()
	{
		if (_queuedItems.Count != 0)
		{
			QueuedItem queuedItem = _queuedItems[0];
			_queuedItems.RemoveAt(0);
			if (queuedItem.ForAdd)
			{
				AddInternal(queuedItem.Collection);
			}
			else
			{
				RemoveInternal(queuedItem.Collection);
			}
		}
	}

	public void Add([NotNull] ToDoCollection collection, bool immediately = false)
	{
		if (!string.IsNullOrEmpty(collection.Key) && FindCollection(collection.Key) == null && !CheckQueuedItems(collection, forAdd: true))
		{
			if (immediately)
			{
				AddInternal(collection, immediately: true);
				return;
			}
			_queuedItems.Add(new QueuedItem
			{
				Collection = collection,
				ForAdd = true
			});
		}
	}

	private void AddInternal([NotNull] ToDoCollection collection, bool immediately = false)
	{
		Collections.Add(collection);
		collection.OnAddItem();
		OnListUpdated(added: true);
		if (this.Added != null)
		{
			this.Added(collection, immediately);
		}
	}

	public void Remove([NotNull] ToDoCollection collection, bool immediately = false)
	{
		if (!CheckQueuedItems(collection, forAdd: false) && Collections.Contains(collection))
		{
			if (collection.IsMessageOnly() || immediately)
			{
				RemoveInternal(collection, immediately);
				Collections.Remove(collection);
				OnListUpdated();
			}
			else
			{
				_queuedItems.Add(new QueuedItem
				{
					Collection = collection,
					ForAdd = false
				});
			}
		}
	}

	private bool CheckQueuedItems(ToDoCollection collection, bool forAdd)
	{
		for (int num = _queuedItems.Count - 1; num >= 0; num--)
		{
			QueuedItem queuedItem = _queuedItems[num];
			if (queuedItem.Collection == collection)
			{
				if (queuedItem.ForAdd != forAdd)
				{
					_queuedItems.RemoveAt(num);
				}
				return true;
			}
		}
		return false;
	}

	private void RemoveInternal(ToDoCollection collection, bool immediately = false)
	{
		collection.OnRemoveItem();
		if (this.Removed != null)
		{
			this.Removed(collection, immediately);
		}
	}

	public void RemoveAll()
	{
		int i = 0;
		for (int count = Collections.Count; i < count; i++)
		{
			ToDoCollection toDoCollection = Collections[i];
			toDoCollection.OnRemoveItem();
		}
		Collections.Clear();
		_queuedItems.Clear();
		OnListUpdated();
	}

	public ToDoCollection FindCollection(string id)
	{
		for (int i = 0; i < Collections.Count; i++)
		{
			if (Collections[i].Key == id)
			{
				return Collections[i];
			}
		}
		for (int j = 0; j < _queuedItems.Count; j++)
		{
			if (_queuedItems[j].Collection.Key == id)
			{
				return _queuedItems[j].Collection;
			}
		}
		return null;
	}

	public ToDoBase FindToDo(string key)
	{
		for (int i = 0; i < Collections.Count; i++)
		{
			ToDoBase toDoBase = Collections[i].FindToDo(key);
			if (toDoBase != null)
			{
				return toDoBase;
			}
		}
		for (int j = 0; j < _queuedItems.Count; j++)
		{
			ToDoBase toDoBase2 = _queuedItems[j].Collection.FindToDo(key);
			if (toDoBase2 != null)
			{
				return toDoBase2;
			}
		}
		return null;
	}

	public void SetUpdated(ToDoBase todo, bool textOnly = false)
	{
		ToDoCollection toDoCollection = null;
		for (int i = 0; i < Collections.Count; i++)
		{
			if (Collections[i].Has(todo))
			{
				toDoCollection = Collections[i];
				break;
			}
		}
		if (toDoCollection != null && this.ContextUpdated != null)
		{
			this.ContextUpdated(toDoCollection, todo, textOnly);
		}
	}

	public void CallComplete(string key)
	{
		FindToDo(key)?.CallComplete();
	}

	private void OnListUpdated(bool added = false)
	{
		if (added)
		{
			InsertionSort();
		}
		if (this.ListUpdated != null)
		{
			this.ListUpdated();
		}
	}

	private void InsertionSort()
	{
		int count = Collections.Count;
		for (int i = 1; i < count; i++)
		{
			for (int num = i; num >= 1; num--)
			{
				ToDoCollection toDoCollection = Collections[num];
				ToDoCollection toDoCollection2 = Collections[num - 1];
				if (toDoCollection2.NPCType < toDoCollection.NPCType || (toDoCollection2.NPCType == toDoCollection.NPCType && toDoCollection2.Order <= toDoCollection.Order))
				{
					break;
				}
				ToDoCollection value = Collections[num];
				Collections[num] = Collections[num - 1];
				Collections[num - 1] = value;
			}
		}
	}
}
