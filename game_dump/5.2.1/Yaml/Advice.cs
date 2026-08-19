using System.Collections.Generic;
using Shared.Skill;

namespace Yaml;

public class Advice
{
	public Gettext name;

	public Gettext description;

	public Dictionary<Category, int> category_levels;

	public SkillAdvice[] skills;

	public int difficulty;

	public int cooperation;

	public bool recommended;

	public string category;

	public string subcategory;

	public string reward_title_id;

	public Gettext reward_items_name;

	public RewardItem[] reward_items;

	public RequiredSkill required_skill;

	public Gettext[] hints;
}
