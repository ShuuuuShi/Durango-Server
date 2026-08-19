using MsgPack;

namespace Messages;

public struct GetCompletedCraters
{
	public const uint TypeCode = 913u;

	public ulong RegionId;

	public static void Pack(Packer packer, GetCompletedCraters val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(913u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.RegionId);
	}

	public static GetCompletedCraters Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		GetCompletedCraters result = default(GetCompletedCraters);
		result.RegionId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<GetCompletedCraters RegionId={RegionId}>";
	}
}
