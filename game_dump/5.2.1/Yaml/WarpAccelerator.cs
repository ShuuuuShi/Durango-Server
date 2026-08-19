using Newtonsoft.Json;

namespace Yaml;

public class WarpAccelerator
{
	[JsonProperty(PropertyName = "breaktime")]
	public float Breaktime;

	[JsonProperty(PropertyName = "phase_time")]
	public float PhaseTime;

	[JsonProperty(PropertyName = "reward_time")]
	public float RewardTime;

	[JsonProperty(PropertyName = "inactivate_time")]
	public float InactivateTime;

	[JsonProperty(PropertyName = "max_phase")]
	public int MaxPhase;
}
