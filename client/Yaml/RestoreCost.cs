using Newtonsoft.Json;
using Shared.Economy;

namespace Yaml;

public struct RestoreCost
{
	[JsonProperty(PropertyName = "amount", Required = Required.Always)]
	public int Amount;

	[JsonProperty(PropertyName = "currency", Required = Required.Always)]
	public Currency Currency;
}
