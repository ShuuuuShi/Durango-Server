using System.Collections.Generic;
using Yaml.Util;

namespace Yaml;

public class ClanYaml : Singleton<ClanYaml>
{
	public long[] level_thresholds;

	public Dictionary<int, ClanLevelReward> level_rewards;
}
