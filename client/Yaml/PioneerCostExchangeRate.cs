using Newtonsoft.Json;

namespace Yaml;

public class PioneerCostExchangeRate
{
	[JsonProperty(PropertyName = "grade")]
	public int Grade;

	[JsonProperty(PropertyName = "rates")]
	public PioneerRate[] Rates;
}
