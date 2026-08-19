using System.Collections.Generic;
using Newtonsoft.Json;

namespace Durango.Logic.PlayGuide;

public class FlowJson
{
	[JsonProperty(PropertyName = "type")]
	public string Type;

	[JsonProperty(PropertyName = "param")]
	public string Param;

	[JsonProperty(PropertyName = "skip_load")]
	public bool SkipLoad;

	[JsonProperty(PropertyName = "can_restart")]
	public bool CanRestart;

	[JsonProperty(PropertyName = "region")]
	public FlowRegion Region;

	[JsonProperty(PropertyName = "flow")]
	public List<string> Flow;
}
