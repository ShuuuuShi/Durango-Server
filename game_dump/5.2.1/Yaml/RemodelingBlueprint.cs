using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public class RemodelingBlueprint
{
	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "min_level")]
	public int MinLevel;

	[JsonProperty(PropertyName = "max_level")]
	public int MaxLevel;

	[JsonProperty(PropertyName = "postprocess_time")]
	public int PostprocessTime;

	[JsonProperty(PropertyName = "tool_tags")]
	public Dictionary<string, int> ToolTags;

	[JsonProperty(PropertyName = "slots")]
	public BlueprintSlot[] Slots;
}
