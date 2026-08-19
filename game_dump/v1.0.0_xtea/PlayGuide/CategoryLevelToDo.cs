using Shared.Skill;

namespace PlayGuide;

public class CategoryLevelToDo : ToDoBase
{
	private readonly Category _targetCategory;

	private readonly int _targetLevel;

	public CategoryLevelToDo(string category, int level)
	{
		_targetCategory = ((!string.IsNullOrEmpty(category)) ? category.ToEnum(Category.Survival) : Category.Invalid);
		_targetLevel = level;
	}

	public override void OnAddItem()
	{
		GameSystem<SkillSystem>.Instance().CategoryLevelChanged += CategoryLevelToDo_CategoryLevelChanged;
		int categoryLevel = GameSystem<SkillSystem>.Instance().GetCategoryLevel(_targetCategory);
		CategoryLevelToDo_CategoryLevelChanged(_targetCategory, categoryLevel, categoryLevel);
	}

	public override void OnRemoveItem()
	{
		GameSystem<SkillSystem>.Instance().CategoryLevelChanged -= CategoryLevelToDo_CategoryLevelChanged;
	}

	private void CategoryLevelToDo_CategoryLevelChanged(Category category, int prev, int cur)
	{
		if ((_targetCategory == Category.Invalid || _targetCategory == category) && cur >= _targetLevel)
		{
			CallComplete();
		}
	}
}
