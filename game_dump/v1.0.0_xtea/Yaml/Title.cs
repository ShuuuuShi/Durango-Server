using System.Collections.Generic;
using Shared.Ability;
using Shared.Skill;

namespace Yaml;

public class Title
{
	public Gettext name;

	public Gettext description;

	public Dictionary<Basic, int> abilities;

	public Dictionary<Category, int> category_levels;

	public RequiredSkill[] skills;

	public bool for_advisor;

	public int exptected_level_of_achieved;

	public string icon;
}
