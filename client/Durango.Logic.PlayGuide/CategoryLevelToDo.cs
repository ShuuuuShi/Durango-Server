using Durango.Logic.Skill;
using Durango.Utils.Extensions;
using Shared.Skill;

namespace Durango.Logic.PlayGuide;

public class CategoryLevelToDo : ToDoBase
{
	private readonly Shared.Skill.Category _targetCategory;

	private readonly int _targetLevel;

	public CategoryLevelToDo(string category, int level)
	{
		_targetCategory = ((!string.IsNullOrEmpty(category)) ? category.ToEnum(Shared.Skill.Category.Survival) : Shared.Skill.Category.Invalid);
		_targetLevel = level;
	}

	public override void OnAddItem()
	{
		GameSystem<SkillSystem>.Instance().CategoryLevelChanged += CategoryLevelToDo_CategoryLevelChanged;
		Durango.Logic.Skill.Category skillCategory = GameSystem<SkillSystem>.Instance().GetSkillCategory(_targetCategory);
		CategoryLevelToDo_CategoryLevelChanged(skillCategory);
	}

	public override void OnRemoveItem()
	{
		GameSystem<SkillSystem>.Instance().CategoryLevelChanged -= CategoryLevelToDo_CategoryLevelChanged;
	}

	private void CategoryLevelToDo_CategoryLevelChanged(Durango.Logic.Skill.Category cat)
	{
		if ((_targetCategory == Shared.Skill.Category.Invalid || _targetCategory == cat.Type) && cat.Level >= _targetLevel)
		{
			CallComplete();
		}
	}
}
