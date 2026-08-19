using MsgPack;

namespace Messages;

public struct JoinClan
{
	public const uint TypeCode = 3655u;

	public ulong ClanId;

	public static void Pack(Packer packer, JoinClan val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(3655u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		packer.Pack(val.ClanId);
	}

	public static JoinClan Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		JoinClan result = default(JoinClan);
		result.ClanId = ((MessagePackObject)(ref lastReadData)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<JoinClan ClanId={ClanId}>";
	}
}
