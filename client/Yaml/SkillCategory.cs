using System.Collections.Generic;
using Newtonsoft.Json;

namespace Yaml;

public class SkillCategory
{
	[JsonProperty(PropertyName = "exp_needed")]
	public Dictionary<int, int> ExpNeeded;

	[JsonProperty(PropertyName = "research_times")]
	public Dictionary<int, int> ResearchTimes;

	[JsonProperty(PropertyName = "season")]
	public string Season;
}
