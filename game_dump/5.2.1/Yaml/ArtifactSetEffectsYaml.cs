using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Yaml.Util;

namespace Yaml;

public class ArtifactSetEffectsYaml : Singleton<ArtifactSetEffectsYaml>
{
	[JsonProperty(PropertyName = "mood")]
	public Dictionary<string, ArtifactInteriorMood> InteriorMood;

	[JsonProperty(PropertyName = "set")]
	public Dictionary<string, ArtifactInteriorSet> InteriorSet;

	[JsonIgnore]
	public int InteriorSetMinRequiredStatFactor;

	[JsonIgnore]
	public int InteriorMoodMinRequiredStatFactor;

	protected override void OnInitalized()
	{
		base.OnInitalized();
		InteriorSetMinRequiredStatFactor = InteriorSet.Select((KeyValuePair<string, ArtifactInteriorSet> set) => GetRequiredStatFactor(set.Value)).Min();
		InteriorMoodMinRequiredStatFactor = InteriorMood.Select((KeyValuePair<string, ArtifactInteriorMood> mood) => GetRequiredStatFactor(mood.Value)).Min();
	}

	private static int GetRequiredStatFactor(ArtifactInteriorMood mood)
	{
		return mood?.RequiredStatFactor ?? int.MaxValue;
	}

	private static int GetRequiredStatFactor(ArtifactInteriorSet set)
	{
		return set?.RequiredStatFactor ?? int.MaxValue;
	}
}
