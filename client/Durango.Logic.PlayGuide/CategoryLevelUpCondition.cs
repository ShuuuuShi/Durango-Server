using System;
using Durango.Logic.Skill;
using Durango.Utils.Extensions;
using Shared.Skill;

namespace Durango.Logic.PlayGuide;

internal class CategoryLevelUpCondition : FlowCondition
{
	private readonly string _category;

	private readonly int _level;

	public CategoryLevelUpCondition(string param)
	{
		if (!string.IsNullOrEmpty(param))
		{
			string[] array = param.Split(':');
			_category = array[0];
			if (array.Length >= 2)
			{
				_level = array[1].ToInt();
			}
		}
	}

	protected override void OnRegister()
	{
		GameSystem<SkillSystem>.Instance().CategoryLevelChanged += CategoryLevelUpCondition_CategoryLevelChanged;
		Shared.Skill.Category cat = _category.ToEnum(Shared.Skill.Category.Survival);
		Durango.Logic.Skill.Category skillCategory = GameSystem<SkillSystem>.Instance().GetSkillCategory(cat);
		CategoryLevelUpCondition_CategoryLevelChanged(skillCategory);
	}

	protected override void OnUnregister()
	{
		GameSystem<SkillSystem>.Instance().CategoryLevelChanged -= CategoryLevelUpCondition_CategoryLevelChanged;
	}

	private void CategoryLevelUpCondition_CategoryLevelChanged(Durango.Logic.Skill.Category cat)
	{
		if (string.Compare(cat.Type.ToString(), _category, StringComparison.OrdinalIgnoreCase) == 0 && cat.Level >= _level)
		{
			Interrupt();
		}
	}
}
