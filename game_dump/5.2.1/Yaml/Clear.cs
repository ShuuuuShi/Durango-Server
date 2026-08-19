using Newtonsoft.Json;

namespace Yaml;

public struct Clear
{
	[JsonProperty(PropertyName = "days_after")]
	public int DaysAfter;

	[JsonProperty(PropertyName = "reward_amount")]
	public int RewardAmount;
}
