using Shared.Ability;

namespace Yaml;

public class SkillModifier
{
	public StatType type;

	public string icon;

	public string reduce_type;

	public Gettext description;

	public Gettext name;

	public IncreaseType increase_type;

	public ApplyType apply_type;

	public bool inverse;
}
