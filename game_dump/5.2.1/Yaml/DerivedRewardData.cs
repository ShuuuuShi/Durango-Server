using Newtonsoft.Json;

namespace Yaml;

public class DerivedRewardData
{
	[JsonProperty(PropertyName = "required_value")]
	public int RequiredValue;

	[JsonProperty(PropertyName = "reward_id")]
	public string RewardId;
}
