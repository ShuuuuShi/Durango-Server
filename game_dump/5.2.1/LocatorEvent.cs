using Durango.Logic;
using Durango.Logic.PlayGuide;
using Durango.UI;
using Durango.UI.PlayGuide.ClickTarget;
using Durango.Utils.Extensions;
using Shared.Attendance;

public class LocatorEvent : LocatorMenu
{
	private EventGroup _eventGroup;

	private CategoryType _targetCategory;

	private int _rewardIndex;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_eventGroup = UIManager.FindScript<EventGroup>();
		SetMenuType(MenuType.Event);
		Parameter parameter = Parameters.Get("get_reward");
		if (parameter != null)
		{
			_targetCategory = parameter.id.ToEnum(CategoryType.Invalid);
			int.TryParse(parameter.param, out _rewardIndex);
		}
	}

	protected override string SelectPhase()
	{
		if (_eventGroup != null && _eventGroup.IsOpened)
		{
			if (_eventGroup.GetCurrenCategoryType() != _targetCategory)
			{
				return "select_category";
			}
			return "touch_reward";
		}
		return base.SelectPhase();
	}

	protected override void UpdateTargetTransform()
	{
		string currentPhase = base.CurrentPhase;
		if (!(currentPhase == "select_category"))
		{
			if (currentPhase == "touch_reward")
			{
				base.TargetTransform = _eventGroup.GetCategoryNodeWidget(_rewardIndex);
			}
			else
			{
				base.UpdateTargetTransform();
			}
		}
		else
		{
			base.TargetTransform = _eventGroup.GetCategoryTransform(_targetCategory);
		}
	}
}
