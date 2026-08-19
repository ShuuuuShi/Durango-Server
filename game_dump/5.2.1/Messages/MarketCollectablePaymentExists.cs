using MsgPack;

namespace Messages;

public struct MarketCollectablePaymentExists
{
	public const uint TypeCode = 5110u;

	public bool Exists;

	public static void Pack(Packer packer, MarketCollectablePaymentExists val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5110u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Exists);
	}

	public static MarketCollectablePaymentExists Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MarketCollectablePaymentExists result = default(MarketCollectablePaymentExists);
		result.Exists = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<MarketCollectablePaymentExists Exists={Exists}>";
	}
}
