using Newtonsoft.Json;

namespace Yaml;

public struct Battle
{
	[JsonProperty(PropertyName = "hungry_ratio_enter_battle", Required = Required.Always)]
	public float HungryRatioEnterBattle;
}
