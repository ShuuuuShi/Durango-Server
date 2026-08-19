using MsgPack;

namespace Messages;

public struct AcceptPurchase
{
	public const uint TypeCode = 5247809u;

	public string PurchaseId;

	public string SubId;

	public static void Pack(Packer packer, AcceptPurchase val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(5247809u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.PurchaseId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.PurchaseId);
		}
		if (val.SubId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SubId);
		}
	}

	public static AcceptPurchase Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AcceptPurchase result = default(AcceptPurchase);
		result.PurchaseId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.SubId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<AcceptPurchase PurchaseId={PurchaseId} SubId={SubId}>";
	}
}
