using Newtonsoft.Json;

namespace Yaml;

public struct PeriodicCountsLimit
{
	[JsonProperty(PropertyName = "days")]
	public int Days;

	[JsonProperty(PropertyName = "counts")]
	public int Counts;
}
