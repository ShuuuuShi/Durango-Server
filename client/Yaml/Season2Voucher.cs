using Newtonsoft.Json;

namespace Yaml;

public struct Season2Voucher
{
	[JsonProperty(PropertyName = "amount")]
	public int Amount;

	[JsonProperty(PropertyName = "voucher_id")]
	public string Id;

	[JsonProperty(PropertyName = "commodity_id")]
	public string CommodityId;
}
