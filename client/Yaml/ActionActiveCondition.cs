using Newtonsoft.Json;
using Shared.Battle;

namespace Yaml;

public class ActionActiveCondition
{
	[JsonProperty(PropertyName = "active_type")]
	public ActiveType ActiveType;

	[JsonProperty(PropertyName = "value")]
	public string Value;

	[JsonProperty(PropertyName = "duration")]
	public float Duration;
}
