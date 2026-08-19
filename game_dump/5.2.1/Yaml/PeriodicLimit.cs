using Newtonsoft.Json;

namespace Yaml;

public struct PeriodicLimit
{
	[JsonProperty(PropertyName = "days")]
	public int Days;

	[JsonProperty(PropertyName = "renewal_period")]
	public int RenewalPeriod;
}
