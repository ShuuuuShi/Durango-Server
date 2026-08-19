using MsgPack;

namespace Messages;

public struct AppearEntityOnTile
{
	public ulong EntityId;

	public ushort EntityType;

	public Point2 Tile;

	public static void Pack(Packer packer, AppearEntityOnTile val, bool hint = false)
	{
		packer.PackArrayHeader(3);
		packer.Pack(val.EntityId);
		packer.Pack(val.EntityType);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static AppearEntityOnTile Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		AppearEntityOnTile result = default(AppearEntityOnTile);
		result.EntityId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EntityType = ((MessagePackObject)(ref lastReadData2)).AsUInt16();
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		return result;
	}

	public override string ToString()
	{
		return $"<AppearEntityOnTile EntityId={EntityId} EntityType={EntityType} Tile={Tile}>";
	}
}
