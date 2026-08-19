using Newtonsoft.Json;
using Shared.Region;

namespace Yaml;

public class PutInContainerInfo
{
	[JsonProperty(PropertyName = "biomes", Required = Required.Always)]
	public Biome[] Biomes;

	[JsonProperty(PropertyName = "tags", Required = Required.Always)]
	public string[] Tags;
}
