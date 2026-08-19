using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public struct Build
{
	[JsonProperty(PropertyName = "condition_scales", Required = Required.Always)]
	public Dictionary<int, float> ConditionoScales;
}
