using MsgPack;

namespace Messages;

public struct Touch
{
	public const uint TypeCode = 2021u;

	public string EntityId;

	public Point2 Tile;

	public ushort EntityType;

	public static void Pack(Packer packer, Touch val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(2021u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		if (val.EntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.EntityId);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack(val.EntityType);
	}

	public static Touch Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Touch result = default(Touch);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		unpacker.Read();
		result.EntityType = unpacker.LastReadData.AsUInt16();
		return result;
	}

	public override string ToString()
	{
		return $"<Touch EntityId={EntityId} Tile={Tile} EntityType={EntityType}>";
	}
}
