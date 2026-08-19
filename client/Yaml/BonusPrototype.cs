using Newtonsoft.Json;

namespace Yaml;

public class BonusPrototype
{
	[JsonProperty(PropertyName = "prototype_id")]
	public string PrototypeId;

	[JsonProperty(PropertyName = "count")]
	public int Count;
}
