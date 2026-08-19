using Newtonsoft.Json;

namespace Yaml;

public struct Explorer
{
	[JsonProperty(PropertyName = "search_cooltime", Required = Required.Always)]
	public int SearchCooltime;
}
