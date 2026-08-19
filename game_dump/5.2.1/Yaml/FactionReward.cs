using System.Collections.Generic;
using Newtonsoft.Json;
using Shared.Economy;

namespace Yaml;

public class FactionReward
{
	[JsonProperty(PropertyName = "money")]
	public Dictionary<Currency, int> Money;

	[JsonProperty(PropertyName = "title_id")]
	public string TitleId;
}
