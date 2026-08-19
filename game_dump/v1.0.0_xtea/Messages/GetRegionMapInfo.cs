using MsgPack;

namespace Messages;

public struct GetRegionMapInfo
{
	public const uint TypeCode = 205u;

	public ulong RegionId;

	public static void Pack(Packer packer, GetRegionMapInfo val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(205u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.RegionId);
	}

	public static GetRegionMapInfo Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetRegionMapInfo result = default(GetRegionMapInfo);
		result.RegionId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<GetRegionMapInfo RegionId={RegionId}>";
	}
}
