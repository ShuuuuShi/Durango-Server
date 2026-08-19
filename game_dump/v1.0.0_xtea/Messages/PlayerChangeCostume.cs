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
		//IL_000a: Unknown result type (might be due to invalid IL or missing references)
		//IL_000f: Unknown result type (might be due to invalid IL or missing references)
		//IL_003a: Unknown result type (might be due to invalid IL or missing references)
		//IL_003f: Unknown result type (might be due to invalid IL or missing references)
		unpacker.Read();
		MessagePackObject lastReadData = unpacker.LastReadData;
		PlayerChangeCostume result = default(PlayerChangeCostume);
		result.SentAt = ((MessagePackObject)(ref lastReadData)).AsDouble();
		unpacker.Read();
		result.PlayerInfo = PlayerInfo.Unpack(unpacker);
		unpacker.Read();
		MessagePackObject lastReadData2 = unpacker.LastReadData;
		result.Name = ((MessagePackObject)(ref lastReadData2)).AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<PlayerChangeCostume SentAt={SentAt} PlayerInfo={PlayerInfo} Name={Name}>";
	}
}
