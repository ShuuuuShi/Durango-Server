using Newtonsoft.Json;

namespace Yaml;

public struct PrototypePresetTag
{
	[JsonProperty(PropertyName = "id")]
	public string Id;

	[JsonProperty(PropertyName = "level")]
	public int Level;
}
