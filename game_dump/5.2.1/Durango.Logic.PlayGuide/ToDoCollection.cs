using System;
using System.Collections.Generic;
using Durango.UI.Control;
using JetBrains.Annotations;
using L10N;
using Messages;
using UnityEngine;

namespace Durango.Logic.PlayGuide;

public class ToDoCollection
{
	public struct Detail
	{
		public bool IsHeaderVisible;

		public bool IsTodoListVisible;

		public SyncString CommonText;

		public NGUIText.Alignment CommonTextAlignment;

		public string ButtonText;

		public Action ButtonClicked;

		public PresetButton.Style ButtonStyle;

		public PresetButton.Effect ButtonEffect;

		public Pair<int, int>? Progress;
	}

	public string Title;

	public string Icon;

	public int IconSize;

	public readonly List<ToDoBase> ToDoList = new List<ToDoBase>();

	public string Season;

	public string SubIcon;

	[CanBeNull]
	public GuideEvent GuideEvent;

	private Action _clicked;

	private Action _helpClicked;

	private float _addedTime;

	private float _removedTime;

	private string _key;

	public bool IsSubIconRotational { get; protected set; }

	public bool IsDisabled { get; protected set; }

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
			num = Time.time - _removedTime;
			return Mathf.Min(1f, 1f - num);
		}
	}

	public bool HasHelp => _helpClicked != null;

	public virtual Detail? GetDetail()
	{
		Detail value = default(Detail);
		if (IsDisabled)
		{
			value.IsHeaderVisible = true;
			value.CommonTextAlignment = NGUIText.Alignment.Left;
			value.CommonText = GetMessage();
		}
		else
		{
			value.IsHeaderVisible = true;
			value.IsTodoListVisible = true;
		}
		return value;
	}

	public virtual bool IsMessageOnly()
	{
		return KUtility.GetSize(ToDoList) == 0;
	}

	public virtual void Update()
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

	public virtual void OnAddItem()
	{
		_addedTime = Time.time;
		_removedTime = 0f;
		int i = 0;
		for (int size = KUtility.GetSize(ToDoList); i < size; i++)
		{
			ToDoList[i].OnAddItem();
		}
	}

	public virtual void OnRemoveItem()
	{
		_addedTime = 0f;
		_removedTime = Time.time + 1f;
		int i = 0;
		for (int size = KUtility.GetSize(ToDoList); i < size; i++)
		{
			ToDoList[i].OnRemoveItem();
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

	public virtual SyncString GetMessage()
	{
		return T._("이 섬에서 진행할 수 없습니다.");
	}

	public virtual string[] GetNavigationKey()
	{
		return new string[1] { Key };
	}

	public void NotifyClicked()
	{
		if (_clicked != null)
		{
			_clicked();
		}
	}

	public void SetClicked(Action action)
	{
		_clicked = action;
	}

	public void NotifyHelpClicked()
	{
		if (_helpClicked != null)
		{
			_helpClicked();
		}
	}

	protected void SetHelpClicked(Action action)
	{
		_helpClicked = action;
	}

	public virtual string GetSubIcon()
	{
		if (string.IsNullOrEmpty(SubIcon))
		{
			Season? season = GameSystem<SeasonSystem>.Instance().GetSeason(Season);
			if (season.HasValue)
			{
				return season.Value.IconSmall;
			}
			return null;
		}
		return SubIcon;
	}

	public virtual bool IsPlaySound()
	{
		return true;
	}
}
