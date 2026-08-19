using MsgPack;

namespace Messages;

public struct Timer
{
	public const uint TypeCode = 1134u;

	public float Duration;

	public static void Pack(Packer packer, Timer val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1134u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.Duration);
	}

	public static Timer Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		Timer result = default(Timer);
		result.Duration = ((MessagePackObject)(ref lastReadData)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<Timer Duration={Duration}>";
	}
}
