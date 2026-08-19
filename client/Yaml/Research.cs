using Newtonsoft.Json;
using Shared.Economy;
using Shared.Laboratory;

namespace Yaml;

public class Research
{
	[JsonProperty(PropertyName = "category")]
	public ResearchCategory Category;

	[JsonProperty(PropertyName = "name")]
	public Gettext Name;

	[JsonProperty(PropertyName = "effect")]
	public ResearchEffect Effect;

	[JsonProperty(PropertyName = "currency")]
	public Currency Currency;

	[JsonProperty(PropertyName = "amount")]
	public int Amount;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "tier")]
	public LaboratoryTier Tier;
}
