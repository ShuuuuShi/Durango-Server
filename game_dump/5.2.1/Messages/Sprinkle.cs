using MsgPack;

namespace Messages;

public struct Sprinkle
{
	public const uint TypeCode = 37121u;

	public string EntityId;

	public Point2 Tile;

	public static void Pack(Packer packer, Sprinkle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(37121u);
		}
		else
		{
			packer.PackArrayHeader(2);
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
	}

	public static Sprinkle Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Sprinkle result = default(Sprinkle);
		result.EntityId = unpacker.LastReadData.AsString();
		unpacker.Read();
		unpacker.ReadUInt16(out var result2);
		result.Tile.x = result2;
		unpacker.ReadUInt16(out result2);
		result.Tile.y = result2;
		return result;
	}

	public override string ToString()
	{
		return $"<Sprinkle EntityId={EntityId} Tile={Tile}>";
	}
}
