using System.Collections.Generic;

namespace PlayGuide;

public class InteractToDo : ToDoBase
{
	private readonly string _targetType;

	private int _interactedCount;

	private HashSet<string> _interactedList;

	public InteractToDo(string target, int count)
	{
		_targetType = target;
		base.TargetProgress = ((count <= 0) ? 1 : count);
		if (string.IsNullOrEmpty(target))
		{
			_interactedList = new HashSet<string>();
		}
	}

	public override void OnAddItem()
	{
		GameSystem<InteractionSystem>.Instance().OnTouchItemSucceed += InteractToDo_OnTouchItemSucceed;
	}

	public override void OnRemoveItem()
	{
		GameSystem<InteractionSystem>.Instance().OnTouchItemSucceed -= InteractToDo_OnTouchItemSucceed;
	}

	private void InteractToDo_OnTouchItemSucceed(string target)
	{
		if (string.IsNullOrEmpty(_targetType))
		{
			_interactedList.Add(target);
			CallProgressChange(_interactedList.Count);
		}
		else if (_targetType == target)
		{
			_interactedCount++;
			CallProgressChange(_interactedCount);
		}
	}
}
