using Newtonsoft.Json;

namespace Yaml;

public class ReformTechSupportTag
{
	[JsonProperty(PropertyName = "min_level")]
	public int MinLevel;

	[JsonProperty(PropertyName = "max_level")]
	public int MaxLevel;
}
