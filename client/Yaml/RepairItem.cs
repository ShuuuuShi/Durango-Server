using Newtonsoft.Json;

namespace Yaml;

public struct RepairItem
{
	[JsonProperty(PropertyName = "durability_result", Required = Required.Always)]
	public DurabilityResult DurabilityResult;

	[JsonProperty(PropertyName = "limit_durability", Required = Required.Always)]
	public float LimitDurability;
}
