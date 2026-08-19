using System;
using Newtonsoft.Json;

namespace Yaml;

public struct Market
{
	[JsonProperty(PropertyName = "listing_fee_rate")]
	public double ListingFeeRate;

	[JsonProperty(PropertyName = "sales_fee_threshold")]
	public int SalesFeeThreshold;

	[JsonProperty(PropertyName = "sales_fee_rates")]
	public SalesFeeRates SalesFeeRates;

	public long GetListingFee(long price)
	{
		return Math.Max(0L, (long)((double)price * ListingFeeRate));
	}

	public long GetSalesFee(long price)
	{
		long num = Math.Min(SalesFeeThreshold, price);
		long num2 = price - num;
		return (long)Math.Max(0.0, (double)num * SalesFeeRates.UnderThreshold + (double)num2 * SalesFeeRates.OverThreshold);
	}
}
