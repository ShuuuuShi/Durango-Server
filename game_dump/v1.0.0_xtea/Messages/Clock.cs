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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0026: Unknown result type (might be due to invalid IL or missing references)
		//IL_002b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Clock result = default(Clock);
		result.ClientTime = ((MessagePackObject)(ref lastReadData)).AsDouble();
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.ServerTime = ((MessagePackObject)(ref lastReadData2)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<Clock ClientTime={ClientTime} ServerTime={ServerTime}>";
	}
}
