using Newtonsoft.Json;

namespace Yaml;

public struct Durability
{
	[JsonProperty(PropertyName = "daily_velocity", Required = Required.Always)]
	public float DailyVelocity;
}
