using MsgPack;

namespace Messages;

public struct SetCategoryItemOrder
{
	public const uint TypeCode = 3689u;

	public ulong EntityId;

	public Point2 Tile;

	public string Category;

	public ulong[] ItemOrder;

	public static void Pack(Packer packer, SetCategoryItemOrder val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(3689u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.Category == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Category);
		}
		if (val.ItemOrder == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.ItemOrder.Length);
		for (int i = 0; i < val.ItemOrder.Length; i++)
		{
			packer.Pack(val.ItemOrder[i]);
		}
	}

	public static SetCategoryItemOrder Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_005a: Unknown result type (might be due to invalid IL or missing references)
		//IL_005f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0075: Unknown result type (might be due to invalid IL or missing references)
		//IL_007a: Unknown result type (might be due to invalid IL or missing references)
		//IL_00a8: Unknown result type (might be due to invalid IL or missing references)
		//IL_00ad: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SetCategoryItemOrder result = default(SetCategoryItemOrder);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Category = ((MessagePackObject)(ref lastReadData2)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData3)).AsInt32();
		result.ItemOrder = new ulong[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ulong[] itemOrder = result.ItemOrder;
			int num3 = i;
			MessagePackObject lastReadData4 = unpacker.LastReadData;
			itemOrder[num3] = ((MessagePackObject)(ref lastReadData4)).AsUInt64();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SetCategoryItemOrder EntityId={EntityId} Tile={Tile} Category={Category} ItemOrder={ItemOrder}>";
	}
}
