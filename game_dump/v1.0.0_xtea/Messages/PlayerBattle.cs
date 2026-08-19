using MsgPack;

namespace Messages;

public struct PlayerBattle
{
	public const uint TypeCode = 1002u;

	public double SentAt;

	public PlayerInfo PlayerInfo;

	public bool IsAimMode;

	public static void Pack(Packer packer, PlayerBattle val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(1002u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.SentAt);
		PlayerInfo.Pack(packer, val.PlayerInfo);
		packer.Pack(val.IsAimMode);
	}

	public static PlayerBattle Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlayerBattle result = default(PlayerBattle);
		result.SentAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		unpacker.Read();
		result.PlayerInfo = PlayerInfo.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.IsAimMode = ((MessagePackObject)(ref lastReadData2)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerBattle SentAt={SentAt} PlayerInfo={PlayerInfo} IsAimMode={IsAimMode}>";
	}
}
