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
		//IL_0008: Unknown result type (might be due to invalid IL or missing references)
		//IL_000d: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Revive result = default(Revive);
		if (((MessagePackObject)(ref lastReadData)).IsNil)
		{
			result.WarpholeTile = null;
		}
		else
		{
			ushort num = default(ushort);
			unpacker.ReadUInt16(ref num);
			Point2 value = default(Point2);
			value.x = num;
			unpacker.ReadUInt16(ref num);
			value.y = num;
			result.WarpholeTile = value;
		}
		return result;
	}

	public override string ToString()
	{
		return $"<Revive WarpholeTile={WarpholeTile}>";
	}
}
