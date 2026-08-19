using MsgPack;

namespace Messages;

public struct Warp
{
	public const uint TypeCode = 2108u;

	public Point2 Tile;

	public static void Pack(Packer packer, Warp val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2108u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static Warp Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		Warp result = default(Warp);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		return result;
	}

	public override string ToString()
	{
		return $"<Warp Tile={Tile}>";
	}
}
