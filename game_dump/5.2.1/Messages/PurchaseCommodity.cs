using MsgPack;

namespace Messages;

public struct PurchaseCommodity
{
	public const uint TypeCode = 856710u;

	public string CommodityId;

	public static void Pack(Packer packer, PurchaseCommodity val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(856710u);
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

	public static PurchaseCommodity Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PurchaseCommodity result = default(PurchaseCommodity);
		result.CommodityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<PurchaseCommodity CommodityId=" + CommodityId + ">";
	}
}
