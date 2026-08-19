using MsgPack;

namespace Messages;

public struct PlayerChangeCostume
{
	public const uint TypeCode = 1014u;

	public double SentAt;

	public PlayerInfo PlayerInfo;

	public string Name;

	public static void Pack(Packer packer, PlayerChangeCostume val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(4);
			packer.Pack(1014u);
		}
		else
		{
			packer.PackArrayHeader(3);
		}
		packer.Pack(val.SentAt);
		PlayerInfo.Pack(packer, val.PlayerInfo);
		if (val.Name == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.Name);
		}
	}

	public static PlayerChangeCostume Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		PlayerChangeCostume result = default(PlayerChangeCostume);
		result.SentAt = unpacker.LastReadData.AsDouble();
		unpacker.Read();
		result.PlayerInfo = PlayerInfo.Unpack(unpacker);
		unpacker.Read();
		result.Name = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerChangeCostume SentAt={SentAt} PlayerInfo={PlayerInfo} Name={Name}>";
	}
}
