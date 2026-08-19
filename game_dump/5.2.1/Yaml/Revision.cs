using Newtonsoft.Json;

namespace Yaml;

public class Revision
{
	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "finish_at")]
	public string FinishAt;

	[JsonProperty(PropertyName = "starts_at")]
	public string StartsAt;

	[JsonProperty(PropertyName = "reward_acquire_limit_at")]
	public string RewardAcquireLimitAt;
}
