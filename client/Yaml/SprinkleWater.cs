using Newtonsoft.Json;

namespace Yaml;

public struct SprinkleWater
{
	[JsonProperty(PropertyName = "duration", Required = Required.Always)]
	public int Duration;
}
