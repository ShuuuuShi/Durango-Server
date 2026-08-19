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
		string currentPhase = base.CurrentPhase;
		if (!(currentPhase == "select_category"))
		{
			if (currentPhase == "touch_reward")
			{
				base.TargetTransform = _questGroup.GetQuestReceiveButtonTransform(_todoId);
			}
			else
			{
				base.UpdateTargetTransform();
			}
		}
		else
		{
			base.TargetTransform = _questGroup.GetQuestMenuTabTransform(_category);
		}
	}
}
