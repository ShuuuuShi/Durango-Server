using MsgPack;

namespace Messages;

public struct TargetEntityPosition
{
	public const uint TypeCode = 3951u;

	public Point2? Tile;

	public static void Pack(Packer packer, TargetEntityPosition val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3951u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (!val.Tile.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.Tile.Value.x);
		packer.Pack((ushort)val.Tile.Value.y);
	}

	public static TargetEntityPosition Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		TargetEntityPosition result = default(TargetEntityPosition);
		if (unpacker.LastReadData.IsNil)
		{
			result.Tile = null;
		}
		else
		{
			unpacker.ReadUInt16(out var result2);
			Point2 value = default(Point2);
			value.x = result2;
			unpacker.ReadUInt16(out result2);
			value.y = result2;
			result.Tile = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<TargetEntityPosition Tile={Tile}>";
	}
}
