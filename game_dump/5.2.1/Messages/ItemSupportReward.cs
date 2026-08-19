using MsgPack;

namespace Messages;

public struct ItemSupportReward
{
	public Item Item;

	public int Count;

	public static void Pack(Packer packer, ItemSupportReward val, bool hint = false)
	{
		packer.PackArrayHeader(2);
		Item.Pack(packer, val.Item);
		packer.Pack(val.Count);
	}

	public static ItemSupportReward Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ItemSupportReward result = default(ItemSupportReward);
		result.Item = Item.Unpack(unpacker);
		unpacker.Read();
		result.Count = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ItemSupportReward Item={Item} Count={Count}>";
	}
}
