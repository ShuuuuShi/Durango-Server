using Newtonsoft.Json;

namespace Yaml;

public struct War
{
	[JsonProperty(PropertyName = "max_tuner_count", Required = Required.Always)]
	public int MaxTunerCount;
}
