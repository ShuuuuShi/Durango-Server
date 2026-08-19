using Newtonsoft.Json;

namespace Yaml;

public struct ShopContents
{
	[JsonProperty(PropertyName = "items")]
	public ItemContent[] Items;

	[JsonProperty(PropertyName = "money")]
	public MoneyContent[] Money;

	[JsonProperty(PropertyName = "status_effects")]
	public StatusEffectsContent[] StatusEffects;

	[JsonProperty(PropertyName = "motions")]
	public string[] Motions;

	[JsonProperty(PropertyName = "capsulated_modulars")]
	public ModularArtifactContent[] Modulars;

	[JsonProperty(PropertyName = "vouchers")]
	public VoucherContent[] Vouchers;

	[JsonProperty(PropertyName = "refill_vouchers")]
	public VoucherContent[] RefillVouchers;

	[JsonProperty(PropertyName = "motion_ids")]
	public string[] WeightedMotions;

	[JsonProperty(PropertyName = "weighted_items")]
	public WeightedItemContent[] WeightedItems;

	public bool HasContents()
	{
		if (KUtility.GetSize(Items) <= 0 && KUtility.GetSize(Money) <= 0 && KUtility.GetSize(StatusEffects) <= 0 && KUtility.GetSize(Motions) <= 0 && KUtility.GetSize(Modulars) <= 0 && KUtility.GetSize(Vouchers) <= 0 && KUtility.GetSize(RefillVouchers) <= 0 && KUtility.GetSize(WeightedMotions) <= 0)
		{
			return KUtility.GetSize(WeightedItems) > 0;
		}
		return true;
	}

	public bool HasRandomContents()
	{
		if (KUtility.GetSize(WeightedMotions) <= 0)
		{
			return KUtility.GetSize(WeightedItems) > 0;
		}
		return true;
	}
}
