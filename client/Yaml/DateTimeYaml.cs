using Newtonsoft.Json;

namespace Yaml;

public class DateTimeYaml
{
	[JsonProperty(PropertyName = "daytime")]
	public int Daytime;

	[JsonProperty(PropertyName = "sunrise")]
	public int[] Sunrise;

	[JsonProperty(PropertyName = "sunset")]
	public int[] Sunset;
}
