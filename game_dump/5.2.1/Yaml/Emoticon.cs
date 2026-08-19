using Newtonsoft.Json;

namespace Yaml;

public struct Emoticon
{
	[JsonProperty(PropertyName = "id")]
	public string Id;

	[JsonProperty(PropertyName = "default")]
	public bool Default;

	[JsonProperty(PropertyName = "free")]
	public bool Free;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;
}
