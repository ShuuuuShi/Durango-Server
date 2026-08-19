using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public class PrototypePresetPerformance
{
	[JsonProperty(PropertyName = "id")]
	public string Id;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "nums")]
	public Dictionary<string, float> Nums;

	[JsonProperty(PropertyName = "strs")]
	public Dictionary<string, string> Strs;
}
