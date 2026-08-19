using Newtonsoft.Json;

namespace Yaml;

public struct ClearTime
{
	[JsonProperty(PropertyName = "days")]
	public int Days;

	[JsonProperty(PropertyName = "reward_amount")]
	public int RewardAmount;
}
