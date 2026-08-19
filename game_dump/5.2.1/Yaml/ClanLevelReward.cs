using Newtonsoft.Json;

namespace Yaml;

public struct ClanLevelReward
{
	[JsonProperty(PropertyName = "description")]
	public Gettext Description;
}
