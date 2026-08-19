using System;
using System.Collections.Generic;
using UnityEngine;

namespace PlayGuide;

public class ToDoCollection
{
	public Action Clicked;

	public string Title;

	public NPCType NPCType;

	public int Order;

	public IList<ToDoBase> ToDoList;

	private float _addedTime;

	private float _removedTime;

	private string _key;

	public string Key
	{
		get
		{
			if (string.IsNullOrEmpty(_key) && ToDoList.Count > 0)
			{
				return ToDoList[0].Key;
			}
			return _key;
		}
		set
		{
			_key = value;
		}
	}

	public bool WillBeRemoved => _removedTime > 0f;

	public bool IsReady
	{
		get
		{
			if (_addedTime <= 0f || WillBeRemoved)
			{
				return false;
			}
			for (int i = 0; i < KUtility.GetSize(ToDoList); i++)
			{
				if (!ToDoList[i].IsCompleted)
				{
					return true;
				}
			}
			return false;
		}
	}

	public float TweenRatio
	{
		get
		{
			float num = Mathf.Clamp01(Time.time - _addedTime);
			if (num < 1f)
			{
				return num;
			}
			if (_removedTime <= 0f)
			{
				return 1f;
			}
			num = (Time.time - _removedTime) * 0.5f;
			return Mathf.Min(1f, 1f - num);
		}
	}

	public bool IsMessageOnly()
	{
		return KUtility.GetSize(ToDoList) == 0;
	}

	public void Update()
	{
		int i = 0;
		for (int size = KUtility.GetSize(ToDoList); i < size; i++)
		{
			ToDoBase toDoBase = ToDoList[i];
			if (!toDoBase.IsCompleted)
			{
				toDoBase.Process();
			}
		}
	}

	public void OnAddItem()
	{
		_addedTime = Time.time;
		_removedTime = 0f;
		int i = 0;
		for (int size = KUtility.GetSize(ToDoList); i < size; i++)
		{
			ToDoBase toDoBase = ToDoList[i];
			toDoBase.OnAddItem();
		}
	}

	public void OnRemoveItem()
	{
		_addedTime = 0f;
		_removedTime = Time.time + 1f;
		int i = 0;
		for (int size = KUtility.GetSize(ToDoList); i < size; i++)
		{
			ToDoBase toDoBase = ToDoList[i];
			toDoBase.OnRemoveItem();
		}
	}

	public ToDoBase FindToDo(string key)
	{
		int i = 0;
		for (int size = KUtility.GetSize(ToDoList); i < size; i++)
		{
			ToDoBase toDoBase = ToDoList[i];
			if (toDoBase.Key == key)
			{
				return toDoBase;
			}
		}
		return null;
	}

	public bool Has(ToDoBase todo)
	{
		int i = 0;
		for (int size = KUtility.GetSize(ToDoList); i < size; i++)
		{
			if (ToDoList[i] == todo)
			{
				return true;
			}
		}
		return false;
	}
}
