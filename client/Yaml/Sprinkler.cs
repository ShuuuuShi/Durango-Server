using Newtonsoft.Json;

namespace Yaml;

public struct Sprinkler
{
	[JsonProperty(PropertyName = "sprinkle_water", Required = Required.Always)]
	public SprinkleWater SprinkleWater;
}
