using MsgPack;

namespace Messages;

public struct PlayerEmoticon
{
	public const uint TypeCode = 1011u;

	public double SentAt;

	public PlayerInfo PlayerInfo;

	public uint EmoticonType;

	public float Power;

	public static void Pack(Packer packer, PlayerEmoticon val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(5);
			packer.Pack(1011u);
		}
		else
		{
			packer.PackArrayHeader(4);
		}
		packer.Pack(val.SentAt);
		PlayerInfo.Pack(packer, val.PlayerInfo);
		packer.Pack(val.EmoticonType);
		packer.Pack(val.Power);
	}

	public static PlayerEmoticon Unpack(Unpacker unpacker)
	{
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		//IL_0056: Unknown result type (might be due to invalid IL or missing references)
		//IL_005b: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlayerEmoticon result = default(PlayerEmoticon);
		result.SentAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		unpacker.Read();
		result.PlayerInfo = PlayerInfo.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.EmoticonType = ((MessagePackObject)(ref lastReadData2)).AsUInt32();
		unpacker.Read();
		MessagePackObject lastReadData3 = unpacker.LastReadData;
		result.Power = ((MessagePackObject)(ref lastReadData3)).AsSingle();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerEmoticon SentAt={SentAt} PlayerInfo={PlayerInfo} EmoticonType={EmoticonType} Power={Power}>";
	}
}
