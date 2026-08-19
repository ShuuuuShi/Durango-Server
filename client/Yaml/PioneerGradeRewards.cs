using Newtonsoft.Json;
using Yaml.Util;

namespace Yaml;

public class PioneerGradeRewards : Singleton<PioneerGradeRewards>
{
	[JsonProperty(PropertyName = "rewards", Required = Required.Always)]
	public PioneerGradeReward[] Rewards;
}
