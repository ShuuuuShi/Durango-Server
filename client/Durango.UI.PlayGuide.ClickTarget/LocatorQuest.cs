using Durango.Logic;
using Durango.Logic.PlayGuide;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorQuest : LocatorMenu
{
	private QuestGroup _questGroup;

	private string _category;

	private string _todoId;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_questGroup = UIManager.FindScript<QuestGroup>();
		SetMenuType(MenuType.Quest);
		Parameter parameter = Parameters.Get("get_reward");
		if (parameter != null)
		{
			_category = parameter.id;
			_todoId = parameter.param;
		}
	}

	protected override string SelectPhase()
	{
		if (_questGroup != null && _questGroup.IsOpened)
		{
			if (_questGroup.SelectedCategory != _category)
			{
				return "select_category";
			}
			return "touch_reward";
		}
		return base.SelectPhase();
	}

	protected override void UpdateTargetTransform()
	{
		switch (base.CurrentPhase)
		{
		case "select_category":
			base.TargetTransform = _questGroup.GetQuestMenuTabTransform(_category);
			break;
		case "touch_reward":
			base.TargetTransform = _questGroup.GetQuestReceiveButtonTransform(_todoId);
			break;
		default:
			base.UpdateTargetTransform();
			break;
		}
	}
}
