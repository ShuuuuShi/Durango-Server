using MsgPack;

namespace Messages;

public struct MarketPaymentReceived
{
	public const uint TypeCode = 5130u;

	public string FirstItemName;

	public int ItemCount;

	public long TotalPrice;

	public static void Pack(Packer packer, MarketPaymentReceived val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(5130u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.PackString(val.FirstItemName);
		packer.Pack(val.ItemCount);
		packer.Pack(val.TotalPrice);
	}

	public static MarketPaymentReceived Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MarketPaymentReceived result = default(MarketPaymentReceived);
		result.FirstItemName = LocalizeSystem.UnpackGettextFromMsgPack(unpacker);
		unpacker.Read();
		result.ItemCount = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.TotalPrice = unpacker.LastReadData.AsInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<MarketPaymentReceived FirstItemName={FirstItemName} ItemCount={ItemCount} TotalPrice={TotalPrice}>";
	}
}
