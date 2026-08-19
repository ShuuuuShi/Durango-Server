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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetClock result = default(GetClock);
		result.Time = ((MessagePackObject)(ref lastReadData)).AsDouble();
		return result;
	}

	public override string ToString()
	{
		return $"<GetClock Time={Time}>";
	}
}
