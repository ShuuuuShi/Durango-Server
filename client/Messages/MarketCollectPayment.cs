using MsgPack;

namespace Messages;

public struct MarketCollectPayment
{
	public const uint TypeCode = 5101u;

	public string ProductId;

	public static void Pack(Packer packer, MarketCollectPayment val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(5101u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.ProductId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ProductId);
		}
	}

	public static MarketCollectPayment Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		MarketCollectPayment result = default(MarketCollectPayment);
		result.ProductId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<MarketCollectPayment ProductId={ProductId}>";
	}
}
