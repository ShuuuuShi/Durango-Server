using Newtonsoft.Json;

namespace Yaml;

public struct Dash
{
	[JsonProperty(PropertyName = "stamina", Required = Required.Always)]
	public int Stamina;
}
