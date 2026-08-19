using MsgPack;

namespace Messages;

public struct RegionExpired
{
	public const uint TypeCode = 2424u;

	public float After;

	public static void Pack(Packer packer, RegionExpired val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2424u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.After);
	}

	public static RegionExpired Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		RegionExpired result = default(RegionExpired);
		result.After = ((MessagePackObject)(ref lastReadData)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<RegionExpired After={After}>";
	}
}
