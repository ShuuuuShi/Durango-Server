using MsgPack;

namespace Messages;

public struct Container
{
	public const uint TypeCode = 2058u;

	public int Capacity;

	public Item[] Contents;

	public static void Pack(Packer packer, Container val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2058u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.Capacity);
		if (val.Contents == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.Contents.Length);
		for (int i = 0; i < val.Contents.Length; i++)
		{
			Item.Pack(packer, val.Contents[i]);
		}
	}

	public static Container Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0024: Unknown result type (might be due to invalid IL or missing references)
		//IL_0029: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Container result = default(Container);
		result.Capacity = ((MessagePackObject)(ref lastReadData)).AsInt32();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.Contents = new Item[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Item reference = ref result.Contents[i];
			reference = Item.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Container Capacity={Capacity} Contents={Contents}>";
	}
}
