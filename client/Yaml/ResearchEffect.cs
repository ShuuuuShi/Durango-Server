using Newtonsoft.Json;
using Shared.Laboratory;

namespace Yaml;

public struct ResearchEffect
{
	[JsonProperty(PropertyName = "status_effect_id")]
	public string StatusEffectId;

	[JsonProperty(PropertyName = "level")]
	public int Level;

	[JsonProperty(PropertyName = "apply_limits")]
	public EffectApplyLimits ApplyLimits;
}
