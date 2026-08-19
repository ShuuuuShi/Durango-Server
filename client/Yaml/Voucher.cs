using System.Collections.Generic;
using Durango.Utils.Extensions;
using L10N;
using Newtonsoft.Json;
using Shared.Voucher;

namespace Yaml;

public struct Voucher
{
	[JsonProperty(PropertyName = "count_max")]
	public int CountMax;

	[JsonProperty(PropertyName = "description")]
	public Gettext Description;

	[JsonProperty(PropertyName = "expires_on")]
	public string ExpiresOn;

	[JsonProperty(PropertyName = "guide_type")]
	public GuideType GuideType;

	[JsonProperty(PropertyName = "icon")]
	public string Icon;

	[JsonProperty(PropertyName = "icon_colors")]
	public List<string> IconColors;

	[JsonProperty(PropertyName = "link")]
	public Gettext Link;

	[JsonProperty(PropertyName = "name", Required = Required.Always)]
	public Gettext Name;

	[JsonProperty(PropertyName = "visible")]
	public bool Visible;

	public string GetHexColor()
	{
		return (IconColors == null) ? "FFFFFF" : IconColors.Get(0, "FFFFFF");
	}

	public bool IsValid()
	{
		return !string.IsNullOrEmpty(Name);
	}

	public string GetIconText()
	{
		return $"[{GetHexColor()}][icon={Icon}][-]";
	}

	public string GetCostFormat(int amount)
	{
		return $"{GetIconText()} {amount}";
	}

	public string GetEmphasisCostFormat(int amount)
	{
		return string.Format(T.Culture, "[preset=round_box?{0}    {1:N0}]", GetIconText(), amount);
	}
}
