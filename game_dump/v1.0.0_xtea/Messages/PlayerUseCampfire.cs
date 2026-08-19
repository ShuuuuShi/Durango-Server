using MsgPack;

namespace Messages;

public struct PlayerUseCampfire
{
	public const uint TypeCode = 1009u;

	public double SentAt;

	public PlayerInfo PlayerInfo;

	public static void Pack(Packer packer, PlayerUseCampfire val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(1009u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		packer.Pack(val.SentAt);
		PlayerInfo.Pack(packer, val.PlayerInfo);
	}

	public static PlayerUseCampfire Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlayerUseCampfire result = default(PlayerUseCampfire);
		result.SentAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		unpacker.Read();
		result.PlayerInfo = PlayerInfo.Unpack(unpacker);
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerUseCampfire SentAt={SentAt} PlayerInfo={PlayerInfo}>";
	}
}
