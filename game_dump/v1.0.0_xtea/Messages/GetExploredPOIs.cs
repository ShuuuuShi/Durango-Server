using MsgPack;

namespace Messages;

public struct GetExploredPOIs
{
	public const uint TypeCode = 902u;

	public ulong RegionId;

	public static void Pack(Packer packer, GetExploredPOIs val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(902u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.RegionId);
	}

	public static GetExploredPOIs Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetExploredPOIs result = default(GetExploredPOIs);
		result.RegionId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<GetExploredPOIs RegionId={RegionId}>";
	}
}
