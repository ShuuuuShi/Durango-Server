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
		unpacker.Read();
		PlayerChangeGender result = default(PlayerChangeGender);
		result.PlayerInfo = PlayerInfo.Unpack(unpacker);
		unpacker.Read();
		result.Male = unpacker.LastReadData.AsBoolean();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerChangeGender PlayerInfo={PlayerInfo} Male={Male}>";
	}
}
