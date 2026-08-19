using Newtonsoft.Json;

namespace Yaml;

public struct Messenger
{
	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "portrait")]
	public string Portrait;
}
