using Newtonsoft.Json;

namespace Yaml;

public class Faction
{
	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "friendship_label")]
	public Gettext FriendshipLabel;

	[JsonProperty(PropertyName = "unknown_text")]
	public Gettext UnknownText;

	[JsonProperty(PropertyName = "help_text")]
	public Gettext HelpText;

	[JsonProperty(PropertyName = "level_thresholds")]
	public int[] LevelThresholds;

	[JsonProperty(PropertyName = "titles")]
	public Gettext[] Titles;

	[JsonProperty(PropertyName = "cooltime")]
	public int Cooltime;

	[JsonProperty(PropertyName = "display_cooltime")]
	public bool DisplayCooltime;

	[JsonProperty(PropertyName = "rewards")]
	public FactionReward[] Rewards;

	[JsonProperty(PropertyName = "season")]
	public string Season;

	[JsonProperty(PropertyName = "open_limit")]
	public OpenLimit OpenLimit;
}
