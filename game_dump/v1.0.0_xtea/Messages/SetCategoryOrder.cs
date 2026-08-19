using MsgPack;

namespace Messages;

public struct SetCategoryOrder
{
	public const uint TypeCode = 3688u;

	public ulong EntityId;

	public Point2 Tile;

	public string[] CategoryOrder;

	public static void Pack(Packer packer, SetCategoryOrder val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(3688u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.CategoryOrder == null)
		{
			packer.PackArrayHeader(0);
			return;
		}
		packer.PackArrayHeader(val.CategoryOrder.Length);
		for (int i = 0; i < val.CategoryOrder.Length; i++)
		{
			if (val.CategoryOrder[i] == null)
			{
				packer.PackString(string.Empty);
			}
			else
			{
				packer.PackString(val.CategoryOrder[i]);
			}
		}
	}

	public static SetCategoryOrder Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		SetCategoryOrder result = default(SetCategoryOrder);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		int num2 = ((MessagePackObject)(ref lastReadData2)).AsInt32();
		result.CategoryOrder = new string[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			string[] categoryOrder = result.CategoryOrder;
			int num3 = i;
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			categoryOrder[num3] = ((MessagePackObject)(ref lastReadData3)).AsString();
		}
		return result;
	}

	public override string ToString()
	{
		return $"<SetCategoryOrder EntityId={EntityId} Tile={Tile} CategoryOrder={CategoryOrder}>";
	}
}
