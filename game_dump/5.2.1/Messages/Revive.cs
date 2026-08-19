using MsgPack;

namespace Messages;

public struct Revive
{
	public const uint TypeCode = 2101u;

	public Point2? WarpholeTile;

	public static void Pack(Packer packer, Revive val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2101u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (!val.WarpholeTile.HasValue)
		{
			packer.PackNull();
			return;
		}
		packer.PackArrayHeader(2);
		packer.Pack((ushort)val.WarpholeTile.Value.x);
		packer.Pack((ushort)val.WarpholeTile.Value.y);
	}

	public static Revive Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Revive result = default(Revive);
		if (unpacker.LastReadData.IsNil)
		{
			result.WarpholeTile = null;
		}
		else
		{
			unpacker.ReadUInt16(out var result2);
			Point2 value = default(Point2);
			value.x = result2;
			unpacker.ReadUInt16(out result2);
			value.y = result2;
			result.WarpholeTile = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Revive WarpholeTile={WarpholeTile}>";
	}
}
