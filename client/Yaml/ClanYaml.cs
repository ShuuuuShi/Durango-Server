using System.Collections.Generic;
using Newtonsoft.Json;
using Yaml.Util;

namespace Yaml;

public class ClanYaml : Singleton<ClanYaml>
{
	[JsonProperty(PropertyName = "level_thresholds")]
	public long[] LevelThresholds;

	[JsonProperty(PropertyName = "level_rewards")]
	public Dictionary<int, ClanLevelReward> LevelRewards;
}
