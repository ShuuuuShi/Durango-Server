using Newtonsoft.Json;

namespace Yaml;

public struct Ally
{
	[JsonProperty(PropertyName = "slot_opens_at", Required = Required.Always)]
	public int[] SlotOpensAt;

	[JsonProperty(PropertyName = "default_slot_count", Required = Required.Always)]
	public int DefaultSlotCount;

	[JsonProperty(PropertyName = "max_slot_count", Required = Required.Always)]
	public int MaxSlotCount;
}
