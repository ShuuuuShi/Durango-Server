using Newtonsoft.Json;

namespace Yaml;

public class ArtifactInteriorMood
{
	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "desc")]
	public Gettext Description;

	[JsonProperty(PropertyName = "summary_desc")]
	public Gettext SummaryDescription;

	[JsonProperty(PropertyName = "total_level")]
	public int TotalLevel;

	[JsonProperty(PropertyName = "required_stat_factor")]
	public int RequiredStatFactor;

	[JsonProperty(PropertyName = "season")]
	public string Season;

	[JsonProperty(PropertyName = "target_prototypes")]
	public string[] TargetPrototypes;
}
