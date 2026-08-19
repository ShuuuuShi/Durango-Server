using Newtonsoft.Json;

namespace Yaml;

public class ClanResearch : Research
{
	[JsonProperty(PropertyName = "duration")]
	public double Duration;
}
