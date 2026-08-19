using Newtonsoft.Json;

namespace Yaml;

public struct MaxLevels
{
	[JsonProperty(PropertyName = "player", Required = Required.Always)]
	public int Player;

	[JsonProperty(PropertyName = "resistance", Required = Required.Always)]
	public int Resistance;
}
