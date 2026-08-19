using MsgPack;

namespace Messages;

public struct LeaveClan
{
	public const uint TypeCode = 3652u;

	public ulong ClanId;

	public static void Pack(Packer packer, LeaveClan val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3652u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.ClanId);
	}

	public static LeaveClan Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		LeaveClan result = default(LeaveClan);
		result.ClanId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<LeaveClan ClanId={ClanId}>";
	}
}
