using MsgPack;

namespace Messages;

public struct CancelClanJoinRequest
{
	public const uint TypeCode = 1923487521u;

	public string ClanId;

	public static void Pack(Packer packer, CancelClanJoinRequest val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(1923487521u);
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

	public static CancelClanJoinRequest Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		CancelClanJoinRequest result = default(CancelClanJoinRequest);
		result.ClanId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return "<CancelClanJoinRequest ClanId=" + ClanId + ">";
	}
}
