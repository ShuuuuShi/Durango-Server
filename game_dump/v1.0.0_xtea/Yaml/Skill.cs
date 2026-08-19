using Shared.Skill;

namespace Yaml;

public class Skill
{
	public Category category;

	public int category_level;

	public Gettext name;

	public string icon;

	public Gettext description;

	public Gettext subcategory;

	public bool untrain_disabled;

	public string precedence;

	public int skill_point;

	public int render_priority;

	public string[] rewards;
}
