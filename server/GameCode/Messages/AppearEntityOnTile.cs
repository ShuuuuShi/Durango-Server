using MsgPack;

namespace Messages;

public struct AppearEntityOnTile
{
	public string EntityId;

	public ushort EntityType;

	public bool IsAlive;

	public Point2 Tile;

	public static void Pack(Packer packer, AppearEntityOnTile val, bool hint = false)
	{
		packer.PackArrayHeader(4);
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.Pack(val.EntityType);
		packer.Pack(val.IsAlive);
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static AppearEntityOnTile Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		AppearEntityOnTile result = default(AppearEntityOnTile);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		result.EntityType = unpacker.LastReadData.AsUInt16();
		unpacker.Read();
		result.IsAlive = unpacker.LastReadData.AsBoolean();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		return result;
	}

	public override string ToString()
	{
		return $"<AppearEntityOnTile EntityId={EntityId} EntityType={EntityType} IsAlive={IsAlive} Tile={Tile}>";
	}
}
