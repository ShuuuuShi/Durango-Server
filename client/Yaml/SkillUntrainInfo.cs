using Newtonsoft.Json;
using Shared.Economy;

namespace Yaml;

public class SkillUntrainInfo
{
	[JsonProperty(PropertyName = "currency")]
	public Currency Currency;

	[JsonProperty(PropertyName = "amount")]
	public int Amount;

	[JsonProperty(PropertyName = "vouchers")]
	public SkillUntrainCost[] Vouchers;
}
