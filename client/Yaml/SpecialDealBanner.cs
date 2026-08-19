using Newtonsoft.Json;

namespace Yaml;

public class SpecialDealBanner
{
	[JsonProperty(PropertyName = "banner_title")]
	public Gettext Title;

	[JsonProperty(PropertyName = "banner_promotion_description")]
	public Gettext PromotionDescription;

	[JsonProperty(PropertyName = "banner_warning_description")]
	public Gettext WarningDescription;

	[JsonProperty(PropertyName = "banner_item_description")]
	public Gettext ItemDescription;
}
