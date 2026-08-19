using MsgPack;

namespace Messages;

public struct ReceivingItem
{
	public double WarpStartsAt;

	public double ReceivingAt;

	public Item Item;

	public static void Pack(Packer packer, ReceivingItem val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack(val.WarpStartsAt);
		packer.Pack(val.ReceivingAt);
		Item.Pack(packer, val.Item);
	}

	public static ReceivingItem Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ReceivingItem result = default(ReceivingItem);
		result.WarpStartsAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.ReceivingAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.Item = Item.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<ReceivingItem WarpStartsAt={WarpStartsAt} ReceivingAt={ReceivingAt} Item={Item}>";
	}
}
