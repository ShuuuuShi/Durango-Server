using Durango.Logic;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorLearningGuide : LocatorMenu
{
	private SkillGroup _skillGroup;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_skillGroup = UIManager.FindScript<SkillGroup>();
		SetMenuType(MenuType.Skill);
	}

	protected override string SelectPhase()
	{
		if (_skillGroup != null && _skillGroup.IsOpened)
		{
			return "learning_guide";
		}
		return base.SelectPhase();
	}

	protected override void UpdateTargetTransform()
	{
		string currentPhase = base.CurrentPhase;
		if (currentPhase != null && currentPhase == "learning_guide")
		{
			base.TargetTransform = _skillGroup.LearningGuideButton;
		}
		else
		{
			base.UpdateTargetTransform();
		}
	}
}
