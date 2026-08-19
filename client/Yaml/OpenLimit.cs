using Newtonsoft.Json;

namespace Yaml;

public class OpenLimit
{
	[JsonProperty(PropertyName = "ends_at")]
	public string EndsAt;

	[JsonProperty(PropertyName = "starts_at")]
	public string StartsAt;
}
