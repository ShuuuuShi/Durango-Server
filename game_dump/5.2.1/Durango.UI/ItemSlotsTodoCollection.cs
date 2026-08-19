using System.Collections.Generic;
using Durango.Logic.Item;
using Durango.Logic.PlayGuide;

namespace Durango.UI;

public abstract class ItemSlotsTodoCollection : ToDoCollection
{
	private int _count;

	protected readonly List<int> SlotCounts = new List<int>();

	protected bool HasTool;

	protected ItemSlotsTodoCollection()
	{
		SetHelpClicked(OnHelpClick);
		IconSize = 40;
	}

	public override string GetSubIcon()
	{
		return "mission_pin";
	}

	public override bool IsPlaySound()
	{
		return false;
	}

	protected void Begin()
	{
		_count = 0;
		if (ToDoList != null)
		{
			ToDoList.Clear();
		}
	}

	protected ItemSlotTodo GetNext()
	{
		ToDoBase toDoBase;
		if (_count < ToDoList.Count)
		{
			toDoBase = ToDoList[_count];
		}
		else
		{
			toDoBase = new ItemSlotTodo();
			ToDoList.Add(toDoBase);
		}
		_count++;
		return (ItemSlotTodo)toDoBase;
	}

	protected void Add(ItemSlot slot)
	{
		GetNext().Set(slot);
	}

	protected void Add(OrTagFilter tool)
	{
		GetNext().Set(tool);
	}

	protected void End()
	{
		if (ToDoList != null && _count < ToDoList.Count)
		{
			ToDoList.RemoveRange(_count, ToDoList.Count - _count);
		}
		Refresh();
	}

	public bool Refresh()
	{
		FillSlotCount();
		if (ToDoList == null)
		{
			return false;
		}
		bool result = false;
		for (int i = 0; i < ToDoList.Count; i++)
		{
			ToDoBase toDoBase = ToDoList[i];
			int num = ((i < SlotCounts.Count) ? SlotCounts[i] : (HasTool ? 1 : 0));
			if (toDoBase.CurrentProgress != num)
			{
				result = true;
				toDoBase.CurrentProgress = num;
				toDoBase.IsCompleted = toDoBase.TargetProgress <= num;
			}
		}
		return result;
	}

	private void OnHelpClick()
	{
		OpenUI();
	}

	protected abstract void FillSlotCount();

	protected abstract void OpenUI();
}
