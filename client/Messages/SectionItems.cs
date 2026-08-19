using MsgPack;

namespace Messages;

public struct SectionItems
{
	public const uint TypeCode = 3693u;

	public Item[] Items;

	public string[] ItemOrder;

	public static void Pack(Packer packer, SectionItems val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(3693u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		if (val.Items == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Items.Length);
			for (int i = 0; i < val.Items.Length; i++)
			{
				Item.Pack(packer, val.Items[i]);
			}
		}
		if (val.ItemOrder == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ItemOrder.Length);
		for (int j = 0; j < val.ItemOrder.Length; j++)
		{
			if (val.ItemOrder[j] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.ItemOrder[j]);
			}
		}
	}

	public static SectionItems Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		SectionItems result = default(SectionItems);
		result.Items = new Item[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.Items[i];
			reference = Item.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result.ItemOrder = new string[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			result.ItemOrder[j] = unpacker.LastReadData.AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SectionItems Items={Items} ItemOrder={ItemOrder}>";
	}
}
