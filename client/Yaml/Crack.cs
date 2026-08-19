using Newtonsoft.Json;

namespace Yaml;

public class Crack
{
	[JsonProperty(PropertyName = "required_voucher_id", Required = Required.Always)]
	public string VoucherId;
}
