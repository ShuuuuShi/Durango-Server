using Newtonsoft.Json;

namespace Yaml;

public class PromotionLink
{
	[JsonProperty(PropertyName = "main_text")]
	public Gettext MainText;

	[JsonProperty(PropertyName = "sub_text")]
	public Gettext SubText;

	[JsonProperty(PropertyName = "hud_text")]
	public Gettext HudText;

	[JsonProperty(PropertyName = "bg_color")]
	public string BackgroundColor;

	[JsonProperty(PropertyName = "image")]
	public string Image;

	[JsonProperty(PropertyName = "commodity_id")]
	public string CommodityId;

	[JsonProperty(PropertyName = "web_link")]
	public string WebLink;

	[JsonProperty(PropertyName = "start_at")]
	public string StartAt;

	[JsonProperty(PropertyName = "end_at")]
	public string EndAt;
}
