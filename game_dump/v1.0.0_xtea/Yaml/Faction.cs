namespace Yaml;

public class Faction
{
	public Gettext name;

	public Gettext friendship_label;

	public int[] level_thresholds;

	public Gettext[] titles;

	public int cooltime;

	public bool display_cooltime;

	public FactionReward[] rewards;
}
