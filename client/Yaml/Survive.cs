using Newtonsoft.Json;

namespace Yaml;

public struct Survive
{
	[JsonProperty(PropertyName = "reward_bonus")]
	public float RewardBonus;

	[JsonProperty(PropertyName = "survived_count")]
	public int SurvivorCount;
}
