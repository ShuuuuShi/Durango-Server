using Newtonsoft.Json;

namespace Yaml;

public struct SupplyLevel
{
	[JsonProperty(PropertyName = "level")]
	public int Level;

	[JsonProperty(PropertyName = "level_up_count")]
	public int? TotalSteps;

	[JsonProperty(PropertyName = "quantity")]
	public int Quantity;
}
