using System.Linq;
using Newtonsoft.Json;

namespace Yaml;

public class OpenMapCost
{
	[JsonProperty(PropertyName = "vouchers")]
	public VoucherWithCommodity[] Vouchers;

	public VoucherWithCommodity GetVoucherFromCommodity()
	{
		return Vouchers.FirstOrDefault((VoucherWithCommodity commodity) => !string.IsNullOrEmpty(commodity.IncludingCommodityId));
	}

	public VoucherWithCommodity GetVoucher()
	{
		return Vouchers.FirstOrDefault((VoucherWithCommodity commodity) => string.IsNullOrEmpty(commodity.IncludingCommodityId));
	}

	public bool HasVoucher()
	{
		string voucherId = GetVoucher().VoucherId;
		if (!string.IsNullOrEmpty(voucherId))
		{
			return InventorySystem.Wallet.GetVoucherCount(voucherId) > 0;
		}
		return true;
	}

	public bool HasVoucherFromCommodity()
	{
		string voucherId = GetVoucherFromCommodity().VoucherId;
		if (!string.IsNullOrEmpty(voucherId))
		{
			return InventorySystem.Wallet.GetVoucherCount(voucherId) > 0;
		}
		return true;
	}
}
