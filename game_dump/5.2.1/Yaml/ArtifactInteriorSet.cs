using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public class ArtifactInteriorSet
{
	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "desc")]
	public Gettext Description;

	[JsonProperty(PropertyName = "summary_desc")]
	public Gettext SummaryDescription;

	[JsonProperty(PropertyName = "tag_slots")]
	public Dictionary<string, int> TagSlots;

	[JsonProperty(PropertyName = "tag_names")]
	public Dictionary<string, Gettext> TagNames;

	[JsonProperty(PropertyName = "required_stat_factor")]
	public int RequiredStatFactor;

	[JsonProperty(PropertyName = "season")]
	public string Season;

	[JsonProperty(PropertyName = "target_prototypes")]
	public string[] TargetPrototypes;
}
