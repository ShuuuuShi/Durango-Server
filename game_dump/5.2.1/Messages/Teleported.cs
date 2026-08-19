using MsgPack;
using Shared.Teleport;

namespace Messages;

public struct Teleported
{
	public const uint TypeCode = 2037u;

	public Point2 Tile;

	public TeleportType Type;

	public static void Pack(Packer packer, Teleported val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(2037u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
		packer.Pack((int)val.Type);
	}

	public static Teleported Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		unpacker.ReadUInt16(out var result);
		Teleported result2 = default(Teleported);
		result2.Tile.x = result;
		unpacker.ReadUInt16(out result);
		result2.Tile.y = result;
		unpacker.Read();
		int num = unpacker.LastReadData.AsInt32();
		if (num < 0 || 100 < num)
		{
			result2.Type = TeleportType.Invalid;
		}
		else
		{
			result2.Type = (TeleportType)num;
		}
		return result2;
	}

	public override string ToString()
	{
		return $"<Teleported Tile={Tile} Type={Type}>";
	}
}
