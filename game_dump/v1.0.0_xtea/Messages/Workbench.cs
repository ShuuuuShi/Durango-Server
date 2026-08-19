using MsgPack;

namespace Messages;

public struct Workbench
{
	public const uint TypeCode = 3000u;

	public ulong EntityId;

	public uint Capacity;

	public Crafting[] Craftings;

	public Item[] CraftedItems;

	public static void Pack(Packer packer, Workbench val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3000u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.EntityId);
		packer.Pack(val.Capacity);
		if (val.Craftings == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.Craftings.Length);
			for (int i = 0; i < val.Craftings.Length; i++)
			{
				Crafting.Pack(packer, val.Craftings[i]);
			}
		}
		if (val.CraftedItems == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.CraftedItems.Length);
		for (int j = 0; j < val.CraftedItems.Length; j++)
		{
			Item.Pack(packer, val.CraftedItems[j]);
		}
	}

	public static Workbench Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0027: Unknown result type (might be due to invalid IL or missing references)
		//IL_002c: Unknown result type (might be due to invalid IL or missing references)
		//IL_0042: Unknown result type (might be due to invalid IL or missing references)
		//IL_0047: Unknown result type (might be due to invalid IL or missing references)
		//IL_0097: Unknown result type (might be due to invalid IL or missing references)
		//IL_009c: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Workbench result = default(Workbench);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Capacity = ((MessagePackObject)(ref lastReadData2)).AsUInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.Craftings = new Crafting[num];
		for (int i = 0; i < num; i++)
		{
			unpacker.Read();
			ref Crafting reference = ref result.Craftings[i];
			reference = Crafting.Unpack(unpacker);
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData4)).AsInt32();
		result.CraftedItems = new Item[num2];
		for (int j = 0; j < num2; j++)
		{
			unpacker.Read();
			ref Item reference2 = ref result.CraftedItems[j];
			reference2 = Item.Unpack(unpacker);
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Workbench EntityId={EntityId} Capacity={Capacity} Craftings={Craftings} CraftedItems={CraftedItems}>";
	}
}
