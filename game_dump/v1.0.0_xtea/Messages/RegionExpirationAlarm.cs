using MsgPack;

namespace Messages;

public struct RegionExpirationAlarm
{
	public const uint TypeCode = 2423u;

	public float After;

	public static void Pack(Packer packer, RegionExpirationAlarm val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2423u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.After);
	}

	public static RegionExpirationAlarm Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RegionExpirationAlarm result = default(RegionExpirationAlarm);
		result.After = ((MessagePackObject)(ref lastReadData)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<RegionExpirationAlarm After={After}>";
	}
}
