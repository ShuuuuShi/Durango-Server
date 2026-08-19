using Newtonsoft.Json;

namespace Yaml;

public class EncyclopediaCategory
{
	[JsonProperty(PropertyName = "order")]
	public int Order;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;
}
