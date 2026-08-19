using Newtonsoft.Json;

namespace Yaml;

public class PioneerGradeReward
{
	[JsonProperty(PropertyName = "grade")]
	public int Grade;

	[JsonProperty(PropertyName = "texts", Required = Required.Always)]
	public PioneerGradeRewardText Texts;
}
