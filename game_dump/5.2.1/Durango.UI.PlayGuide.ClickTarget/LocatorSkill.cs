using Durango.Logic;
using Durango.Logic.PlayGuide;
using Durango.Logic.Skill;
using Durango.Utils.Extensions;
using Shared.Skill;

namespace Durango.UI.PlayGuide.ClickTarget;

public class LocatorSkill : LocatorMenu
{
	private SkillGroup _skillGroup;

	private Shared.Skill.Category _category = Shared.Skill.Category.Invalid;

	private Skill _skill;

	protected override void OnInitialized()
	{
		base.OnInitialized();
		_skillGroup = UIManager.FindScript<SkillGroup>();
		SetMenuType(MenuType.Skill);
		Parameter parameter = Parameters.Get("select_skill");
		if (parameter != null && parameter.id != null)
		{
			string[] array = parameter.id.Split(':');
			string id = array[0].Trim();
			string sub = ((array.Length <= 1) ? "__base__" : array[1].Trim());
			_skill = GameSystem<SkillSystem>.Instance().FindSkill(id, sub);
		}
		Parameter parameter2 = Parameters.Get("select_category");
		if (parameter2 != null)
		{
			_category = parameter2.id.ToEnum(Shared.Skill.Category.Invalid);
		}
		if (_category == Shared.Skill.Category.Invalid)
		{
			_category = ((_skill == null) ? GameSystem<SkillSystem>.Instance().GetMaxLevelCategory() : _skill.Category);
		}
	}

	protected override string SelectPhase()
	{
		if (_skillGroup != null && _skillGroup.IsOpened)
		{
			if (_skill != null)
			{
				Node selectedSkillNode = _skillGroup.SelectedSkillNode;
				if (selectedSkillNode != null && selectedSkillNode.Id == _skill.Id && selectedSkillNode.Sub == _skill.SubId && selectedSkillNode.Level == _skill.Level + 1)
				{
					return "learn_button";
				}
				if (_skillGroup.SelectedSkillGroup != null)
				{
					if (_skillGroup.SelectedSkillGroup.Name == _skill.Bundle.Group)
					{
						if (base.CurrentPhase != "select_skill")
						{
							_skillGroup.ScrollToSkill(_skill.Id, _skill.SubId, _skill.Level + 1);
						}
						return "select_skill";
					}
					return "select_skill_group";
				}
			}
			if (_skillGroup.SelectedCategory == _category)
			{
				return "category_button";
			}
			return "select_category";
		}
		return base.SelectPhase();
	}

	protected override void UpdateTargetTransform()
	{
		switch (base.CurrentPhase)
		{
		case "select_category":
			base.TargetTransform = _skillGroup.GetCategoryTransform(_category);
			break;
		case "category_button":
			base.TargetTransform = _skillGroup.GetCategoryInfoButtonTransform();
			base.CurrentParameter.rotate = 90f;
			break;
		case "select_skill_group":
			base.TargetTransform = _skillGroup.GetSkillListNodeTransform(_skill.Bundle.Group);
			break;
		case "select_skill":
			base.TargetTransform = _skillGroup.GetSkillNodeTransform(_skill.Id, _skill.SubId, _skill.Level + 1);
			base.CurrentParameter.rotate = 90f;
			break;
		case "learn_button":
			base.TargetTransform = _skillGroup.GetLearnButtonTransform();
			base.CurrentParameter.rotate = 90f;
			break;
		default:
			base.UpdateTargetTransform();
			break;
		}
	}
}
