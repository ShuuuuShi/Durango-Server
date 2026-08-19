using Newtonsoft.Json;

namespace Yaml;

public struct SkillUntrain
{
	[JsonProperty(PropertyName = "free_count", Required = Required.Always)]
	public int FreeCount;

	[JsonProperty(PropertyName = "max_count", Required = Required.Always)]
	public int MaxCount;
}
