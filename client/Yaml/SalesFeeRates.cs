using Newtonsoft.Json;

namespace Yaml;

public struct SalesFeeRates
{
	[JsonProperty(PropertyName = "under_threshold", Required = Required.Always)]
	public double UnderThreshold;

	[JsonProperty(PropertyName = "over_threshold", Required = Required.Always)]
	public double OverThreshold;
}
