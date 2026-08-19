using Newtonsoft.Json;
using Shared.Display;

namespace Yaml;

public class Accessory
{
	[JsonProperty(PropertyName = "id")]
	public string Id;

	[JsonProperty(PropertyName = "model")]
	public string Model;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "type")]
	public AccessoryType Type;
}
