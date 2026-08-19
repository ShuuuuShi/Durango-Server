using System;
using System.Collections.Generic;
using Durango.Logic.Faction;
using Durango.Logic.PlayGuide;
using Durango.Logic.WarpRush;
using JetBrains.Annotations;
using UnityEngine;

public class ToDoListSystem : GameSystem<ToDoListSystem>
{
	private struct QueuedItem
	{
		public Durango.Logic.PlayGuide.ToDoCollection Collection;

		public bool ForAdd;
	}

	private readonly List<Durango.Logic.PlayGuide.ToDoCollection> _collections = new List<Durango.Logic.PlayGuide.ToDoCollection>();

	private readonly List<QueuedItem> _queuedItems = new List<QueuedItem>();

	private Durango.Logic.PlayGuide.ToDoCollection _lastTouchedCollection;

	private float _lastRemovedTime;

	public int CollectionCount => _collections.Count;

	public event Action<Durango.Logic.PlayGuide.ToDoCollection, bool> Added;

	public event Action<Durango.Logic.PlayGuide.ToDoCollection, bool> Removed;

	public event Action<int> ListUpdated;

	public event Action<Durango.Logic.PlayGuide.ToDoCollection, ToDoBase, bool> ContextUpdated;

	public Durango.Logic.PlayGuide.ToDoCollection GetCollection(int index)
	{
		return _collections[index];
	}

	public bool IsAllEmpty()
	{
		if (CollectionCount == 0)
		{
			return _queuedItems.Count == 0;
		}
		return false;
	}

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
		for (int num = _collections.Count - 1; num >= 0; num--)
		{
			Durango.Logic.PlayGuide.ToDoCollection toDoCollection = _collections[num];
			toDoCollection.Update();
			float tweenRatio = toDoCollection.TweenRatio;
			if (tweenRatio < 1f || toDoCollection.WillBeRemoved)
			{
				result = false;
			}
			if (tweenRatio < 0f)
			{
				_collections.RemoveAt(num);
				flag = true;
				_lastRemovedTime = Time.time;
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
		if (_queuedItems.Count == 0)
		{
			return;
		}
		QueuedItem queuedItem = _queuedItems[0];
		if (queuedItem.ForAdd)
		{
			if (_lastRemovedTime + 0.4f <= Time.time)
			{
				_queuedItems.RemoveAt(0);
				AddInternal(queuedItem.Collection);
			}
		}
		else
		{
			_queuedItems.RemoveAt(0);
			RemoveInternal(queuedItem.Collection);
		}
	}

	public void Add([NotNull] Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately = false)
	{
		if (!string.IsNullOrEmpty(collection.Key) && FindCollection(collection.Key) == null && !GameManager.Region.IsPvpIsland() && (!GameManager.Region.IsWarpRush() || collection is Durango.Logic.WarpRush.ToDoCollection) && !CheckQueuedItems(collection, forAdd: true))
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

	private void AddInternal([NotNull] Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately = false)
	{
		_collections.Add(collection);
		collection.OnAddItem();
		OnListUpdated(added: true);
		if (this.Added != null)
		{
			this.Added(collection, immediately);
		}
	}

	public void Remove([NotNull] Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately = false)
	{
		if (!CheckQueuedItems(collection, forAdd: false) && _collections.Contains(collection))
		{
			if (collection.IsMessageOnly() || immediately)
			{
				RemoveInternal(collection, immediately);
				_collections.Remove(collection);
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

	private bool CheckQueuedItems(Durango.Logic.PlayGuide.ToDoCollection collection, bool forAdd)
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

	private void RemoveInternal(Durango.Logic.PlayGuide.ToDoCollection collection, bool immediately = false)
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
		for (int count = _collections.Count; i < count; i++)
		{
			_collections[i].OnRemoveItem();
		}
		_collections.Clear();
		_queuedItems.Clear();
		OnListUpdated();
	}

	public Durango.Logic.PlayGuide.ToDoCollection FindCollection(string id)
	{
		for (int i = 0; i < _collections.Count; i++)
		{
			if (_collections[i].Key == id)
			{
				return _collections[i];
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
		for (int i = 0; i < _collections.Count; i++)
		{
			ToDoBase toDoBase = _collections[i].FindToDo(key);
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

	public void SetUpdated(Durango.Logic.PlayGuide.ToDoCollection collection, bool textOnly = false)
	{
		if (this.ContextUpdated != null)
		{
			this.ContextUpdated(collection, null, textOnly);
		}
	}

	public void SetUpdated(ToDoBase todo, bool textOnly = false)
	{
		Durango.Logic.PlayGuide.ToDoCollection toDoCollection = null;
		for (int i = 0; i < _collections.Count; i++)
		{
			if (_collections[i].Has(todo))
			{
				toDoCollection = _collections[i];
				break;
			}
		}
		if (toDoCollection != null && this.ContextUpdated != null)
		{
			this.ContextUpdated(toDoCollection, todo, textOnly);
		}
	}

	public void Touch(string key)
	{
		ToDoBase toDoBase = FindToDo(key);
		if (toDoBase != null)
		{
			SetUpdated(toDoBase);
			_lastTouchedCollection = FindCollection(key);
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
			int num = _collections.IndexOf(_lastTouchedCollection);
			if (num == -1 && _queuedItems.Count == 0)
			{
				num = CollectionCount - 1;
			}
			if (num >= 0 && KUtility.GetSize(_collections[num].ToDoList) == 0)
			{
				num = -1;
			}
			this.ListUpdated(num);
			_lastTouchedCollection = null;
		}
	}

	private void InsertionSort()
	{
		int count = _collections.Count;
		for (int i = 1; i < count; i++)
		{
			for (int num = i; num >= 1; num--)
			{
				Durango.Logic.PlayGuide.ToDoCollection toDoCollection = _collections[num];
				Durango.Logic.PlayGuide.ToDoCollection toDoCollection2 = _collections[num - 1];
				bool flag = toDoCollection2.IsDisabled && !toDoCollection.IsDisabled;
				if (!flag && GetCollectionOrder(toDoCollection2) < GetCollectionOrder(toDoCollection))
				{
					flag = true;
				}
				if (!flag)
				{
					break;
				}
				Durango.Logic.PlayGuide.ToDoCollection value = _collections[num];
				_collections[num] = _collections[num - 1];
				_collections[num - 1] = value;
			}
		}
	}

	private static int GetCollectionOrder(Durango.Logic.PlayGuide.ToDoCollection collection)
	{
		if (collection is MissionToDoCollection)
		{
			return 0;
		}
		if (collection is EntryTodoCollection)
		{
			return 2;
		}
		return 1;
	}

	[ExposedInEditor(null)]
	private void UpdateTweenTest()
	{
		ToDoListSystem toDoListSystem = GameSystem<ToDoListSystem>.Instance();
		int i = 0;
		for (int collectionCount = toDoListSystem.CollectionCount; i < collectionCount; i++)
		{
			Durango.Logic.PlayGuide.ToDoCollection collection = toDoListSystem.GetCollection(i);
			if (collection.ToDoList == null)
			{
				continue;
			}
			using List<ToDoBase>.Enumerator enumerator = collection.ToDoList.GetEnumerator();
			if (enumerator.MoveNext())
			{
				enumerator.Current.CallComplete();
				GameSystem<ToDoListSystem>.Instance().Remove(collection);
				break;
			}
		}
	}

	[ExposedInEditor(null)]
	private void AddCollectionByNPCType()
	{
		GameSystem<ToDoListSystem>.Instance().RemoveAll();
		int num = Enum.GetNames(typeof(NPCType)).Length;
		for (int i = 0; i < num; i++)
		{
			Durango.Logic.PlayGuide.ToDoCollection toDoCollection = new Durango.Logic.PlayGuide.ToDoCollection();
			NPCType nPCType = (NPCType)i;
			toDoCollection.Icon = nPCType.ToDoIcon();
			toDoCollection.Title = "테스트용 할 일: " + i;
			toDoCollection.Key = nPCType.ToString();
			int num2 = Math.Min(i, 4);
			for (int j = 0; j < num2; j++)
			{
				GatherItemToDo item = new GatherItemToDo(1)
				{
					LocalText = "아이템 찾아주기 하하하 " + j,
					Key = string.Concat(nPCType, ".gather_", j)
				};
				toDoCollection.ToDoList.Add(item);
			}
			GameSystem<ToDoListSystem>.Instance().Add(toDoCollection);
		}
	}

	[ExposedInEditor(null)]
	private void AddCollectionByNPCType2()
	{
		for (int i = 0; i < 1; i++)
		{
			Durango.Logic.PlayGuide.ToDoCollection toDoCollection = new Durango.Logic.PlayGuide.ToDoCollection();
			NPCType nPCType = (NPCType)i;
			toDoCollection.Icon = nPCType.ToDoIcon();
			toDoCollection.Title = "테스트용 할 일: " + i;
			toDoCollection.Key = nPCType.ToString();
			int num = Math.Min(i, 4);
			for (int j = 0; j < num; j++)
			{
				GatherItemToDo item = new GatherItemToDo(1)
				{
					LocalText = "아이템 찾아주기 하하하 " + j,
					Key = string.Concat(nPCType, ".gather_", j)
				};
				toDoCollection.ToDoList.Add(item);
			}
			GameSystem<ToDoListSystem>.Instance().Add(toDoCollection);
		}
	}
}
