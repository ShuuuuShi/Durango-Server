using MsgPack;

namespace Messages;

public struct PurchaseCommodityWithVoucher
{
	public const uint TypeCode = 841253u;

	public string CommodityId;

	public static void Pack(Packer packer, PurchaseCommodityWithVoucher val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(841253u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.CommodityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CommodityId);
		}
	}

	public static PurchaseCommodityWithVoucher Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PurchaseCommodityWithVoucher result = default(PurchaseCommodityWithVoucher);
		result.CommodityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<PurchaseCommodityWithVoucher CommodityId=" + CommodityId + ">";
	}
}
