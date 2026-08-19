using MsgPack;

namespace Messages;

public struct PlayerChangeGender
{
	public const uint TypeCode = 1018u;

	public PlayerInfo PlayerInfo;

	public bool Male;

	public static void Pack(Packer packer, PlayerChangeGender val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(3);
			packer.Pack(1018u);
		}
		else
		{
			packer.PackArrayHeader(2);
		}
		PlayerInfo.Pack(packer, val.PlayerInfo);
		packer.Pack(val.Male);
	}

	public static PlayerChangeGender Unpack(Unpacker unpacker)
	{
		//IL_001e: Unknown result type (might be due to invalid IL or missing references)
		//IL_0023: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		PlayerChangeGender result = default(PlayerChangeGender);
		result.PlayerInfo = PlayerInfo.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		result.Male = ((MessagePackObject)(ref lastReadData)).AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerChangeGender PlayerInfo={PlayerInfo} Male={Male}>";
	}
}
