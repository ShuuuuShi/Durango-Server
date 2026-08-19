using System.Collections.Generic;
using Newtonsoft.Json;
using Yaml.Util;

namespace Yaml;

public class Commodities : Singleton<Commodities>
{
	[JsonProperty(PropertyName = "promotion_links")]
	public PromotionLink[] PromotionLinks;

	[JsonProperty(PropertyName = "posted_commodities")]
	public Dictionary<string, Commodity> PostedCommodities;

	[JsonProperty(PropertyName = "test_commodities")]
	public Dictionary<string, Commodity> TestCommodities;
}
