using MsgPack;

namespace Messages;

public struct GetClock
{
	public const uint TypeCode = 4000u;

	public double Time;

	public static void Pack(Packer packer, GetClock val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(4000u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Time);
	}

	public static GetClock Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		GetClock result = default(GetClock);
		result.Time = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<GetClock Time={Time}>";
	}
}
