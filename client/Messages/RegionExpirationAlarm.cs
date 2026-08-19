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
		unpacker.Read();
		RegionExpirationAlarm result = default(RegionExpirationAlarm);
		result.After = unpacker.LastReadData.AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<RegionExpirationAlarm After={After}>";
	}
}
