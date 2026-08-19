using Newtonsoft.Json;

namespace Yaml;

public class Taming
{
	[JsonProperty(PropertyName = "tamable_hp_rate", Required = Required.Always)]
	public float TamableHpRate;

	[JsonProperty(PropertyName = "taming_cooltime")]
	public float TamingCooltime;
}
