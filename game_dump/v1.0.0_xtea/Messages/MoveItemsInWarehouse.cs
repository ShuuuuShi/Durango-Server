using MsgPack;

namespace Messages;

public struct MoveItemsInWarehouse
{
	public const uint TypeCode = 3685u;

	public ulong EntityId;

	public Point2 Tile;

	public ulong[] ItemIds;

	public string SourceCategory;

	public string TargetCategory;

	public static void Pack(Packer packer, MoveItemsInWarehouse val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(6);
			packer.Pack(3685u);
		}
		else
		{
			packer.PackArrayHeader(5);
		}
		packer.Pack(val.EntityId);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		if (val.ItemIds == null)
		{
			packer.PackArrayHeader(0);
		}
		else
		{
			packer.PackArrayHeader(val.ItemIds.Length);
			for (int i = 0; i < val.ItemIds.Length; i++)
			{
				packer.Pack(val.ItemIds[i]);
			}
		}
		if (val.SourceCategory == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.SourceCategory);
		}
		if (val.TargetCategory == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.TargetCategory);
		}
	}

	public static MoveItemsInWarehouse Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0058: Unknown result type (might be due to invalid IL or missing references)
		//IL_005d: Unknown result type (might be due to invalid IL or missing references)
		//IL_008b: Unknown result type (might be due to invalid IL or missing references)
		//IL_0090: Unknown result type (might be due to invalid IL or missing references)
		//IL_00af: Unknown result type (might be due to invalid IL or missing references)
		//IL_00b4: Unknown result type (might be due to invalid IL or missing references)
		//IL_00cc: Unknown result type (might be due to invalid IL or missing references)
		//IL_00d1: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		MoveItemsInWarehouse result = default(MoveItemsInWarehouse);
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
		result.ItemIds = new ulong[num2];
		for (int i = 0; i < num2; i++)
		{
			unpacker.Read();
			ulong[] itemIds = result.ItemIds;
			int num3 = i;
			MessagePackObject lastReadData3 = unpacker.LastReadData;
			itemIds[num3] = ((MessagePackObject)(ref lastReadData3)).AsUInt64();
		}
		unpacker.Read();
		MessagePackObject lastReadData4 = unpacker.LastReadData;
		result.SourceCategory = ((MessagePackObject)(ref lastReadData4)).AsString();
		unpacker.Read();
		MessagePackObject lastReadData5 = unpacker.LastReadData;
		result.TargetCategory = ((MessagePackObject)(ref lastReadData5)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<MoveItemsInWarehouse EntityId={EntityId} Tile={Tile} ItemIds={ItemIds} SourceCategory={SourceCategory} TargetCategory={TargetCategory}>";
	}
}
