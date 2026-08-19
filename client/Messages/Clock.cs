using MsgPack;

namespace Messages;

public struct Clock
{
	public const uint TypeCode = 4001u;

	public double ClientTime;

	public double ServerTime;

	public static void Pack(Packer packer, Clock val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(4001u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.ClientTime);
		packer.Pack(val.ServerTime);
	}

	public static Clock Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		Clock result = default(Clock);
		result.ClientTime = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.ServerTime = unpacker.LastReadData.AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Clock ClientTime={ClientTime} ServerTime={ServerTime}>";
	}
}
