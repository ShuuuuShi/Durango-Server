using MsgPack;

namespace Messages;

public struct PlayerCharge
{
	public const uint TypeCode = 1015u;

	public double SentAt;

	public PlayerInfo PlayerInfo;

	public string Target;

	public static void Pack(Packer packer, PlayerCharge val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(1015u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.SentAt);
		PlayerInfo.Pack(packer, val.PlayerInfo);
		if (val.Target == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Target);
		}
	}

	public static PlayerCharge Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlayerCharge result = default(PlayerCharge);
		result.SentAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.PlayerInfo = PlayerInfo.Unpack(unpacker);
		unpacker.Read();
		result.Target = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerCharge SentAt={SentAt} PlayerInfo={PlayerInfo} Target={Target}>";
	}
}
