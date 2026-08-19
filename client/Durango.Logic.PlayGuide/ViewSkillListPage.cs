using Durango.UI;
using Durango.Utils.Extensions;
using Shared.Skill;

namespace Durango.Logic.PlayGuide;

public class ViewSkillListPage : ToDoBase
{
	private Category _category;

	public ViewSkillListPage(string category)
	{
		_category = ((!string.IsNullOrEmpty(category)) ? category.ToEnum(Category.Invalid) : Category.Invalid);
	}

	public override void OnAddItem()
	{
		SkillGroup skillGroup = UIManager.FindScript<SkillGroup>();
		skillGroup.SkillListPageShowed += SkillGroup_SkillListPageShowed;
		if (_category == Category.Invalid)
		{
			_category = GameSystem<SkillSystem>.Instance().GetMaxLevelCategory();
		}
	}

	public override void OnRemoveItem()
	{
		SkillGroup skillGroup = UIManager.FindScript<SkillGroup>();
		if (!(skillGroup == null))
		{
			skillGroup.SkillListPageShowed -= SkillGroup_SkillListPageShowed;
		}
	}

	private void SkillGroup_SkillListPageShowed(Category cat)
	{
		if (cat == _category)
		{
			CallComplete();
		}
	}
}
