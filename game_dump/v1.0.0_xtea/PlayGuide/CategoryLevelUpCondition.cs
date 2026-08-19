using System;
using Shared.Skill;

namespace PlayGuide;

internal class CategoryLevelUpCondition : FlowCondition
{
	private readonly string _category;

	private readonly int _level;

	public CategoryLevelUpCondition(string param)
	{
		string[] array = param.Split(':');
		_category = array[0];
		if (array.Length >= 2)
		{
			_level = array[1].ToInt();
		}
	}

	protected override void OnRegister()
	{
		GameSystem<SkillSystem>.Instance().CategoryLevelChanged += CategoryLevelUpCondition_CategoryLevelChanged;
	}

	protected override void OnUnregister()
	{
		GameSystem<SkillSystem>.Instance().CategoryLevelChanged -= CategoryLevelUpCondition_CategoryLevelChanged;
	}

	private void CategoryLevelUpCondition_CategoryLevelChanged(Category category, int prev, int current)
	{
		if (string.Compare(category.ToString(), _category, StringComparison.OrdinalIgnoreCase) == 0 && current >= _level)
		{
			Interrupt();
		}
	}
}
