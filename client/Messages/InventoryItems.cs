using MsgPack;

namespace Messages;

public struct InventoryItems
{
	public const uint TypeCode = 107u;

	public string EntityId;

	public Item[] Items;

	public static void Pack(Packer packer, InventoryItems val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(107u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		if (val.Items == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Items.Length);
		for (int i = 0; i < val.Items.Length; i++)
		{
			Item.Pack(packer, val.Items[i]);
		}
	}

	public static InventoryItems Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		InventoryItems result = default(InventoryItems);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		result.Items = new Item[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.Items[i];
			reference = Item.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<InventoryItems EntityId={EntityId} Items={Items}>";
	}
}
