using Newtonsoft.Json;

namespace Yaml;

public struct VoucherWithCommodity
{
	[JsonProperty(PropertyName = "including_commodity_id")]
	public string IncludingCommodityId;

	[JsonProperty(PropertyName = "voucher_id")]
	public string VoucherId;
}
