using Newtonsoft.Json;

namespace Yaml;

public struct PurchaseLimit
{
	[JsonProperty(PropertyName = "max_count")]
	public int MaxCount;

	[JsonProperty(PropertyName = "is_show_period")]
	public bool IsShowPeriod;

	[JsonProperty(PropertyName = "purchasable_times")]
	public PurchasableTime[] PurchasableTimes;

	[JsonProperty(PropertyName = "periodic_counts_limit")]
	public PeriodicCountsLimit PeriodicCountsLimit;

	[JsonProperty(PropertyName = "periodic_limit")]
	public PeriodicLimit PeriodicLimit;

	[JsonProperty(PropertyName = "steam_dlc_only")]
	public bool IsSteamDlcOnly;
}
