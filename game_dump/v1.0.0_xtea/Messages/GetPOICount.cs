using MsgPack;

namespace Messages;

public struct GetPOICount
{
	public const uint TypeCode = 900u;

	public ulong RegionId;

	public static void Pack(Packer packer, GetPOICount val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(900u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.RegionId);
	}

	public static GetPOICount Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetPOICount result = default(GetPOICount);
		result.RegionId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<GetPOICount RegionId={RegionId}>";
	}
}
