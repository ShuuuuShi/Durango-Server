using MsgPack;

namespace Messages;

public struct GetDiscoverDistances
{
	public const uint TypeCode = 2310u;

	public ulong RegionId;

	public static void Pack(Packer packer, GetDiscoverDistances val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(2310u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.RegionId);
	}

	public static GetDiscoverDistances Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetDiscoverDistances result = default(GetDiscoverDistances);
		result.RegionId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<GetDiscoverDistances RegionId={RegionId}>";
	}
}
