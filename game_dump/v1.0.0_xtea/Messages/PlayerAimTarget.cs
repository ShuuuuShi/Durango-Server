using MsgPack;

namespace Messages;

public struct PlayerAimTarget
{
	public const uint TypeCode = 1010u;

	public double SentAt;

	public PlayerInfo PlayerInfo;

	public ulong Target;

	public static void Pack(Packer packer, PlayerAimTarget val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(1010u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.SentAt);
		PlayerInfo.Pack(packer, val.PlayerInfo);
		packer.Pack(val.Target);
	}

	public static PlayerAimTarget Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlayerAimTarget result = default(PlayerAimTarget);
		result.SentAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		unpacker.Read();
		result.PlayerInfo = PlayerInfo.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Target = ((MessagePackObject)(ref lastReadData2)).AsUInt64();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerAimTarget SentAt={SentAt} PlayerInfo={PlayerInfo} Target={Target}>";
	}
}
