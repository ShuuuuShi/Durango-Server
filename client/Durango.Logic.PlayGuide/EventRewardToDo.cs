using Durango.Logic.Event;
using Durango.Utils.Extensions;
using Shared.Attendance;

namespace Durango.Logic.PlayGuide;

public class EventRewardToDo : ToDoBase
{
	private readonly CategoryType _category;

	private readonly int _index;

	public EventRewardToDo(string category, int index)
	{
		_category = category.ToEnum(CategoryType.Invalid);
		_index = index;
	}

	public override void OnAddItem()
	{
		if (CheckRewardCompleted())
		{
			CallComplete();
		}
		else
		{
			GameSystem<EventSystem>.Instance().CalendarUpdated += EventSystem_CalendarUpdated;
		}
	}

	public override void OnRemoveItem()
	{
		GameSystem<EventSystem>.Instance().CalendarUpdated -= EventSystem_CalendarUpdated;
	}

	private bool CheckRewardCompleted()
	{
		EventSystem eventSystem = GameSystem<EventSystem>.Instance();
		if (eventSystem.Calendars == null)
		{
			return false;
		}
		RewardState rewardState = eventSystem.GetRewardState(_category, _index);
		return rewardState == RewardState.Completed || rewardState == RewardState.None;
	}

	private void EventSystem_CalendarUpdated()
	{
		if (CheckRewardCompleted())
		{
			CallComplete();
		}
	}
}
