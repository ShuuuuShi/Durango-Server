using MsgPack;

namespace Messages;

public struct KickPartyMember
{
	public const uint TypeCode = 20009u;

	public string MemberEntityId;

	public static void Pack(Packer packer, KickPartyMember val, bool hint = false)
	{
		if (hint)
		{
			packer.PackArrayHeader(2);
			packer.Pack(20009u);
		}
		else
		{
			packer.PackArrayHeader(1);
		}
		if (val.MemberEntityId == null)
		{
			packer.PackString(string.Empty);
		}
		else
		{
			packer.PackString(val.MemberEntityId);
		}
	}

	public static KickPartyMember Unpack(Unpacker unpacker)
	{
		unpacker.Read();
		KickPartyMember result = default(KickPartyMember);
		result.MemberEntityId = unpacker.LastReadData.AsString();
		return result;
	}

	public override string ToString()
	{
		return $"<KickPartyMember MemberEntityId={MemberEntityId}>";
	}
}
