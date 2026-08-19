using Newtonsoft.Json;

namespace Yaml;

public class Musician
{
	[JsonProperty(PropertyName = "max_savable_size")]
	public int MaxSavableSize;

	[JsonProperty(PropertyName = "slot_count")]
	public int SlotCount;
}
