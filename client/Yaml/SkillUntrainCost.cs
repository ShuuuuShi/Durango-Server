using Newtonsoft.Json;

namespace Yaml;

public class SkillUntrainCost : Cost
{
	[JsonProperty(PropertyName = "including_commodity_id")]
	public string IncludingCommodityId;
}
