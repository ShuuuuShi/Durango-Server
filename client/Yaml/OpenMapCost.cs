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
		return string.IsNullOrEmpty(voucherId) || InventorySystem.Wallet.GetVoucherCount(voucherId) > 0;
	}

	public bool HasVoucherFromCommodity()
	{
		string voucherId = GetVoucherFromCommodity().VoucherId;
		return string.IsNullOrEmpty(voucherId) || InventorySystem.Wallet.GetVoucherCount(voucherId) > 0;
	}
}
