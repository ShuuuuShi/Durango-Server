using MsgPack;

namespace Messages;

public struct AcceptableSubPurchase
{
	public string PurchaseId;

	public string CommodityId;

	public string[] AcceptableSubIds;

	public static void Pack(Packer packer, AcceptableSubPurchase val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		if (val.PurchaseId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PurchaseId);
		}
		if (val.CommodityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.CommodityId);
		}
		if (val.AcceptableSubIds == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.AcceptableSubIds.Length);
		for (int i = 0; i < val.AcceptableSubIds.Length; i++)
		{
			if (val.AcceptableSubIds[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.AcceptableSubIds[i]);
			}
		}
	}

	public static AcceptableSubPurchase Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AcceptableSubPurchase result = default(AcceptableSubPurchase);
		result.PurchaseId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.CommodityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.AcceptableSubIds = new string[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			result.AcceptableSubIds[i] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<AcceptableSubPurchase PurchaseId={PurchaseId} CommodityId={CommodityId} AcceptableSubIds={AcceptableSubIds}>";
	}
}
