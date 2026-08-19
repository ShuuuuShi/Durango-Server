using Newtonsoft.Json;

namespace Yaml;

public struct SkillConstants
{
	[JsonProperty(PropertyName = "exp_increase_limit", Required = Required.Always)]
	public int ExpIncreaseLimit;

	[JsonProperty(PropertyName = "research_reduce_time_limit", Required = Required.Always)]
	public float ResearchReduceTimeLimit;

	[JsonProperty(PropertyName = "research_time_reduce", Required = Required.Always)]
	public string ResearchTimeReduce;

	[JsonProperty(PropertyName = "survival_check_period", Required = Required.Always)]
	public int SurvivalCheckPeriod;
}
