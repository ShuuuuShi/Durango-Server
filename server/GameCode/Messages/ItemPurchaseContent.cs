using MsgPack;

namespace Messages;

public struct ItemPurchaseContent
{
	public const uint TypeCode = 71294574u;

	public Item Item;

	public static void Pack(Packer packer, ItemPurchaseContent val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(71294574u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		Item.Pack(packer, val.Item);
	}

	public static ItemPurchaseContent Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ItemPurchaseContent result = default(ItemPurchaseContent);
		result.Item = Item.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<ItemPurchaseContent Item={Item}>";
	}
}
