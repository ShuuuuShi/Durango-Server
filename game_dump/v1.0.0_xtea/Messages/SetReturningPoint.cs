using MsgPack;

namespace Messages;

public struct SetReturningPoint
{
	public const uint TypeCode = 2105u;

	public Point2 Tile;

	public static void Pack(Packer packer, SetReturningPoint val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2105u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.x);
		packer.Pack((ushort)val.Tile.y);
	}

	public static SetReturningPoint Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		ushort num = default(ushort);
		unpacker.ReadUInt16(ref num);
		SetReturningPoint result = default(SetReturningPoint);
		result.Tile.x = num;
		unpacker.ReadUInt16(ref num);
		result.Tile.y = num;
		return result;
	}

	public override string ToString()
	{
		return $"<SetReturningPoint Tile={Tile}>";
	}
}
