using MsgPack;

namespace Messages;

public struct ReceivedItems
{
	public const uint TypeCode = 3810u;

	public ReceivingItem[] ReceivingItems;

	public Item[] _ReceivedItems;

	public int UsingSize;

	public int MaxSize;

	public static void Pack(Packer packer, ReceivedItems val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3810u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		if (val.ReceivingItems == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.ReceivingItems.Length);
			for (int i = 0; i < val.ReceivingItems.Length; i++)
			{
				ReceivingItem.Pack(packer, val.ReceivingItems[i]);
			}
		}
		if (val._ReceivedItems == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val._ReceivedItems.Length);
			for (int j = 0; j < val._ReceivedItems.Length; j++)
			{
				Item.Pack(packer, val._ReceivedItems[j]);
			}
		}
		packer.Pack(val.UsingSize);
		packer.Pack(val.MaxSize);
	}

	public static ReceivedItems Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		ReceivedItems result = default(ReceivedItems);
		result.ReceivingItems = new ReceivingItem[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref ReceivingItem reference = ref result.ReceivingItems[i];
			reference = ReceivingItem.Unpack(unpacker);
		}
		unpacker.Read();
		int num2 = unpacker.LastReadData.AsInt32();
		result._ReceivedItems = new Item[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ref Item reference2 = ref result._ReceivedItems[j];
			reference2 = Item.Unpack(unpacker);
		}
		unpacker.Read();
		result.UsingSize = unpacker.LastReadData.AsInt32();
		unpacker.Read();
		result.MaxSize = unpacker.LastReadData.AsInt32();
		return result;
	}

	public override string ToString()
	{
		return $"<ReceivedItems ReceivingItems={ReceivingItems} _ReceivedItems={_ReceivedItems} UsingSize={UsingSize} MaxSize={MaxSize}>";
	}
}
