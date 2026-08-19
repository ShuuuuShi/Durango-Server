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
		unpacker.ReadUInt16(out var result);
		Warp result2 = default(Warp);
		result2.Tile.x = result;
		unpacker.ReadUInt16(out result);
		result2.Tile.y = result;
		return result2;
	}

	public override string ToString()
	{
		return $"<Warp Tile={Tile}>";
	}
}
