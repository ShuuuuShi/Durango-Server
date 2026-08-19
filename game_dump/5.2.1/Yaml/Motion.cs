using Newtonsoft.Json;
using Shared.Purchaser;

namespace Yaml;

public struct Motion
{
	[JsonProperty(PropertyName = "motion_names")]
	public string[] MotionNames;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "free")]
	public bool Free;

	[JsonProperty(PropertyName = "available")]
	public bool Available;

	[JsonProperty(PropertyName = "payback_mileage")]
	public int PaybackMileage;

	[JsonProperty(PropertyName = "tier")]
	public EmotionTier Tier;
}
