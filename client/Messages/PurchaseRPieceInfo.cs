using MsgPack;

namespace Messages;

public struct PurchaseRPieceInfo
{
	public const uint TypeCode = 19021102u;

	public int PurchasedCount;

	public double ResetCountAt;

	public static void Pack(Packer packer, PurchaseRPieceInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(19021102u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.PurchasedCount);
		packer.Pack(val.ResetCountAt);
	}

	public static PurchaseRPieceInfo Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PurchaseRPieceInfo result = default(PurchaseRPieceInfo);
		result.PurchasedCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.ResetCountAt = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<PurchaseRPieceInfo PurchasedCount={PurchasedCount} ResetCountAt={ResetCountAt}>";
	}
}
