using MsgPack;

namespace Messages;

public struct JoinClan
{
	public const uint TypeCode = 3655u;

	public string ClanId;

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
		if (val.ClanId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.ClanId);
		}
	}

	public static JoinClan Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		JoinClan result = default(JoinClan);
		result.ClanId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<JoinClan ClanId=" + ClanId + ">";
	}
}
