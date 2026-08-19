using MsgPack;

namespace Messages;

public struct PurchaseCommodityWithSteamDlc
{
	public const uint TypeCode = 841260u;

	public string AppId;

	public string CommodityId;

	public string SessionTicket;

	public static void Pack(Packer packer, PurchaseCommodityWithSteamDlc val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(841260u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.AppId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.AppId);
		}
		if (val.CommodityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CommodityId);
		}
		if (val.SessionTicket == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SessionTicket);
		}
	}

	public static PurchaseCommodityWithSteamDlc Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PurchaseCommodityWithSteamDlc result = default(PurchaseCommodityWithSteamDlc);
		result.AppId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.CommodityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.SessionTicket = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PurchaseCommodityWithSteamDlc AppId={AppId} CommodityId={CommodityId} SessionTicket={SessionTicket}>";
	}
}
