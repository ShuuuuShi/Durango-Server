using Newtonsoft.Json;
using Shared.Economy;

namespace Yaml;

public class ReviveImmediatelyCost : OpenMapCost
{
	[JsonProperty(PropertyName = "amount")]
	public int Amount;

	[JsonProperty(PropertyName = "currency")]
	public Currency Currency;
}
