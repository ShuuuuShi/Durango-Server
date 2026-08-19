using Newtonsoft.Json;
using Yaml.Util;

namespace Yaml;

public class PlayerStatistics : Singleton<PlayerStatistics>
{
	public int[] level_thresholds;

	[JsonProperty(PropertyName = "resistance_level_thresholds")]
	public int[] ResistanceExpTable;
}
